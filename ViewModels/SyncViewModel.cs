using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnDToolkit.Helpers;
using DnDToolkit.Models;
using DnDToolkit.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace DnDToolkit.ViewModels
{
    public partial class SyncViewModel : ObservableObject, IDisposable
    {
        private readonly LanSyncService _syncService;
        private readonly ISnackbarService _snackbarService;

        // === 状态属性 ===
        [ObservableProperty] private bool isRunning = false;
        [ObservableProperty] private string pinCode = "----";
        [ObservableProperty] private string serverIp = "0.0.0.0";
        [ObservableProperty] private string statusText = "服务未启动";
        [ObservableProperty] private string btnText = "启动同步服务";
        [ObservableProperty] private string qrCodeContent = "";

        // === UI 控制 ===
        [ObservableProperty] private bool isDebugMode = false;
        [ObservableProperty] private bool hasError = false;
        [ObservableProperty] private string errorMessage = "";

        // === 列表数据 ===
        [ObservableProperty]
        private ObservableCollection<PairedDevice> devicesList = new();

        public ObservableCollection<string> Logs { get; } = new();

        public SyncViewModel(LanSyncService syncService, ISnackbarService snackbarService)
        {
            _syncService = syncService;
            _snackbarService = snackbarService;

            _syncService.OnLog += Service_OnLog;
            _syncService.OnCharacterReceived += Service_OnCharacterReceived;
            _syncService.OnDeviceConnected += (s, e) => LoadDevicesToUi();

            LoadDevicesToUi();
        }

        [RelayCommand]
        private void ToggleService()
        {
            HasError = false;

            if (IsRunning)
            {
                _syncService.Stop();
                IsRunning = false;
                StatusText = "服务已停止";
                BtnText = "启动同步服务";
                PinCode = "----";
                ServerIp = "0.0.0.0";
                QrCodeContent = "";
            }
            else
            {
                try
                {
                    _syncService.Start();
                    IsRunning = true;

                    PinCode = _syncService.CurrentPin;
                    ServerIp = _syncService.LocalIp;
                    StatusText = "正在运行";
                    BtnText = "停止服务";

                    // 引用服务中的常量端口
                    QrCodeContent = $"dndsync://{ServerIp}:{LanSyncService.SyncPort}?pin={PinCode}";
                }
                catch (Exception ex)
                {
                    IsRunning = false;
                    HasError = true;
                    ErrorMessage = "服务启动受阻，请尝试修复防火墙设置。";
                    _snackbarService.Show("启动失败", ex.Message, ControlAppearance.Danger);
                }
            }
        }

        [RelayCommand]
        private void FixFirewall()
        {
            try
            {
                // 调用 Helper 申请权限并配置
                FirewallHelper.AddFirewallRule(LanSyncService.SyncPort);

                HasError = false;
                ErrorMessage = "";
                Logs.Insert(0, "[系统] 防火墙规则添加指令已执行。");

                _snackbarService.Show(
                    "操作完成",
                    "请在弹出的窗口中点击“是”，然后重新启动服务。",
                    ControlAppearance.Success,
                    new SymbolIcon(SymbolRegular.Checkmark24),
                    TimeSpan.FromSeconds(4)
                );
            }
            catch (Exception ex)
            {
                _snackbarService.Show("错误", ex.Message, ControlAppearance.Danger);
            }
        }

        private void LoadDevicesToUi()
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                DevicesList.Clear();
                foreach (var dev in _syncService.PairedDevices)
                {
                    DevicesList.Add(dev);
                }
            });
        }

        [RelayCommand]
        private void DeleteDevice(PairedDevice device)
        {
            if (device == null) return;
            _syncService.RemoveDevice(device.DeviceId);
            LoadDevicesToUi();
        }

        private void Service_OnLog(object? sender, SyncLogEventArgs e)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                Logs.Insert(0, e.Message);
                if (Logs.Count > 100) Logs.RemoveAt(Logs.Count - 1);
            });
        }

        private void Service_OnCharacterReceived(object? sender, EventArgs e)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                _snackbarService.Show(
                    "同步成功",
                    "已从移动端接收并保存角色数据。",
                    ControlAppearance.Success,
                    new SymbolIcon(SymbolRegular.ArrowDownload24),
                    TimeSpan.FromSeconds(4));
            });
        }

        public void Dispose()
        {
            _syncService.Stop();
            _syncService.OnLog -= Service_OnLog;
            _syncService.OnCharacterReceived -= Service_OnCharacterReceived;
        }
    }
}