using DnDToolkit.Helpers;
using DnDToolkit.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DnDToolkit.Services
{
    public class SyncLogEventArgs : EventArgs
    {
        public string Message { get; }
        public bool IsError { get; }
        public SyncLogEventArgs(string message, bool isError = false)
        {
            Message = $"[{DateTime.Now:HH:mm:ss}] {message}";
            IsError = isError;
        }
    }

    public class LanSyncService : IDisposable
    {
        // 公共常量端口，供外部引用
        public const int SyncPort = 12345;

        private readonly ICharacterService _characterService;
        private readonly string _deviceConfigPath;

        private HttpListener? _httpListener;
        private UdpClient? _udpBroadcaster;
        private CancellationTokenSource? _cts;

        public List<PairedDevice> PairedDevices { get; private set; } = new();
        public string CurrentPin { get; private set; } = "";
        public string LocalIp { get; private set; } = "";
        public bool IsRunning { get; private set; } = false;

        // 事件
        public event EventHandler<SyncLogEventArgs>? OnLog;
        public event EventHandler? OnCharacterReceived;
        public event EventHandler? OnDeviceConnected;

        public LanSyncService(ICharacterService characterService)
        {
            _characterService = characterService;
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _deviceConfigPath = Path.Combine(docPath, "DnDToolkit", "paired_devices.json");
            LoadDevices();
        }

        public void Start()
        {
            if (IsRunning) return;

            try
            {
                LocalIp = NetworkHelper.GetLocalIpAddress();
                GeneratePin();
                _cts = new CancellationTokenSource();

                // 1. 启动 HTTP Server
                _httpListener = new HttpListener();
                // 使用通配符 + 绑定，允许外部 IP 访问 (依赖 FirewallHelper 的 urlacl 设置)
                _httpListener.Prefixes.Add($"http://+:{SyncPort}/");
                _httpListener.Start();

                Task.Run(() => HttpLoop(_cts.Token));

                // 2. 启动 UDP 广播
                _udpBroadcaster = new UdpClient();
                _udpBroadcaster.EnableBroadcast = true;
                Task.Run(() => BroadcastLoop(_cts.Token));

                IsRunning = true;
                Log("同步服务已启动，等待连接...");
                Log($"监听地址: http://{LocalIp}:{SyncPort}/");
                Log($"PIN 码: {CurrentPin}");
            }
            catch (HttpListenerException ex) when (ex.ErrorCode == 5)
            {
                Log("启动失败：拒绝访问。请点击界面上的“修复防火墙”按钮进行授权。", true);
                throw;
            }
            catch (Exception ex)
            {
                Log($"启动失败: {ex.Message}", true);
                Stop();
                throw;
            }
        }

        public void Stop()
        {
            _cts?.Cancel();
            _httpListener?.Close();
            _udpBroadcaster?.Close();
            IsRunning = false;
            Log("服务已停止。");
        }

        public void RemoveDevice(string deviceId)
        {
            var dev = PairedDevices.FirstOrDefault(d => d.DeviceId == deviceId);
            if (dev != null)
            {
                PairedDevices.Remove(dev);
                SaveDevices();
            }
        }

        private void GeneratePin()
        {
            var random = new Random();
            CurrentPin = random.Next(1000, 9999).ToString();
        }

        private void Log(string msg, bool isError = false)
        {
            OnLog?.Invoke(this, new SyncLogEventArgs(msg, isError));
        }

        // === UDP 广播 ===
        private async Task BroadcastLoop(CancellationToken token)
        {
            var endpoint = new IPEndPoint(IPAddress.Broadcast, SyncPort);
            var info = new
            {
                Name = Environment.MachineName,
                Ip = LocalIp,
                Port = SyncPort,
                Device = "Windows PC"
            };
            string json = JsonSerializer.Serialize(info);
            byte[] data = Encoding.UTF8.GetBytes(json);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_udpBroadcaster != null)
                        await _udpBroadcaster.SendAsync(data, data.Length, endpoint);
                    await Task.Delay(3000, token);
                }
                catch { /* 忽略网络波动 */ }
            }
        }

        // === HTTP 处理 ===
        private async Task HttpLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _httpListener != null && _httpListener.IsListening)
            {
                try
                {
                    var context = await _httpListener.GetContextAsync();
                    _ = HandleRequestAsync(context);
                }
                catch (HttpListenerException) { break; }
                catch (ObjectDisposedException) { break; }
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            var req = context.Request;
            var res = context.Response;

            try
            {
                string deviceId = req.Headers["X-Device-Id"] ?? "Unknown";
                string deviceName = req.Headers["X-Device-Name"] ?? "Unknown Device";
                string? pin = req.Headers["X-Auth-Pin"];

                // 验证 PIN
                if (pin != CurrentPin)
                {
                    Log($"拒绝未授权连接: {req.RemoteEndPoint} (PIN错误)", true);
                    res.StatusCode = 401;
                    await WriteResponse(res, "Invalid PIN");
                    return;
                }

                // 记录设备
                var knownDevice = PairedDevices.FirstOrDefault(d => d.DeviceId == deviceId);
                if (knownDevice == null)
                {
                    knownDevice = new PairedDevice { DeviceId = deviceId, DeviceName = deviceName, IsTrusted = true };
                    PairedDevices.Add(knownDevice);
                }
                knownDevice.LastConnected = DateTime.Now;
                knownDevice.DeviceName = deviceName;
                SaveDevices();
                OnDeviceConnected?.Invoke(this, EventArgs.Empty);

                string path = req.Url?.AbsolutePath.ToLower() ?? "/";
                string method = req.HttpMethod;

                if (path == "/api/list" && method == "GET")
                {
                    await HandleGetList(res);
                }
                else if (path == "/api/download" && method == "GET")
                {
                    string? id = req.QueryString["id"];
                    await HandleDownload(res, id);
                }
                else if (path == "/api/upload" && method == "POST")
                {
                    await HandleUpload(req, res);
                }
                else
                {
                    res.StatusCode = 404;
                }
            }
            catch (Exception ex)
            {
                Log($"请求处理出错: {ex.Message}", true);
                res.StatusCode = 500;
            }
            finally
            {
                res.Close();
            }
        }

        private async Task HandleGetList(HttpListenerResponse res)
        {
            var list = await _characterService.GetAllCharactersAsync();
            var summary = list.Select(c => new
            {
                c.Id,
                Name = c.Profile.CharacterName,
                Details = $"{c.Profile.Race} | {c.Profile.ClassAndLevel}"
            });
            await WriteResponse(res, JsonSerializer.Serialize(summary));
            Log("移动端获取了角色列表");
        }

        private async Task HandleDownload(HttpListenerResponse res, string? idStr)
        {
            if (string.IsNullOrEmpty(idStr)) { res.StatusCode = 400; return; }
            var list = await _characterService.GetAllCharactersAsync();
            var character = list.FirstOrDefault(c => c.Id.ToString() == idStr);

            if (character != null)
            {
                await WriteResponse(res, JsonSerializer.Serialize(character));
                Log($"发送角色: {character.Profile.CharacterName}");
            }
            else
            {
                res.StatusCode = 404;
            }
        }

        private async Task HandleUpload(HttpListenerRequest req, HttpListenerResponse res)
        {
            using var reader = new StreamReader(req.InputStream, req.ContentEncoding);
            string json = await reader.ReadToEndAsync();
            var character = JsonSerializer.Deserialize<Character>(json);

            if (character != null)
            {
                await _characterService.SaveCharacterAsync(character);
                Log($"收到角色: {character.Profile.CharacterName}");
                OnCharacterReceived?.Invoke(this, EventArgs.Empty);
                await WriteResponse(res, "Success");
            }
            else
            {
                res.StatusCode = 400;
            }
        }

        private async Task WriteResponse(HttpListenerResponse res, string content)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(content);
            res.ContentLength64 = buffer.Length;
            res.ContentType = "application/json";
            await res.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }

        private void LoadDevices()
        {
            if (File.Exists(_deviceConfigPath))
            {
                try
                {
                    string json = File.ReadAllText(_deviceConfigPath);
                    PairedDevices = JsonSerializer.Deserialize<List<PairedDevice>>(json) ?? new List<PairedDevice>();
                }
                catch { PairedDevices = new List<PairedDevice>(); }
            }
        }

        private void SaveDevices()
        {
            try
            {
                string json = JsonSerializer.Serialize(PairedDevices);
                File.WriteAllText(_deviceConfigPath, json);
            }
            catch { }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}