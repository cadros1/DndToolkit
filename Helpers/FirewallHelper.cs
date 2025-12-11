using System;
using System.Diagnostics;
using System.Security.Principal;

namespace DnDToolkit.Helpers
{
    public static class FirewallHelper
    {
        private const string RuleName = "DnDToolkit_Sync";

        /// <summary>
        /// 弹出 UAC 提示框，并尝试添加防火墙规则及 HTTP URL ACL
        /// </summary>
        /// <param name="port">需要开放的端口</param>
        public static void AddFirewallRule(int port)
        {
            string? exePath = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(exePath))
            {
                throw new Exception("无法获取当前程序路径。");
            }

            // 获取当前登录用户，用于 HTTP.sys 授权
            string currentUser = WindowsIdentity.GetCurrent().Name;

            // 1. 防火墙规则命令 (删除旧规则 -> 添加程序规则 -> 添加端口规则)
            string firewallCmd =
                $"netsh advfirewall firewall delete rule name=\"{RuleName}\" & " +
                $"netsh advfirewall firewall add rule name=\"{RuleName}\" dir=in action=allow program=\"{exePath}\" enable=yes profile=private,public & " +
                $"netsh advfirewall firewall add rule name=\"{RuleName}\" dir=in action=allow protocol=TCP localport={port} & " +
                $"netsh advfirewall firewall add rule name=\"{RuleName}\" dir=in action=allow protocol=UDP localport={port}";

            // 2. URL ACL 授权命令 (解决 HttpListener 拒绝访问问题)
            // 使用通配符 + 授权给当前用户
            string urlAclCmd =
                $"netsh http delete urlacl url=http://+:{port}/ & " +
                $"netsh http add urlacl url=http://+:{port}/ user=\"{currentUser}\"";

            // 合并执行
            string finalCmd = $"/C {firewallCmd} & {urlAclCmd}";

            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = finalCmd,
                Verb = "runas", // 核心：请求管理员权限 (UAC)
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            };

            try
            {
                using var process = Process.Start(psi);
                process?.WaitForExit();

                if (process != null && process.ExitCode != 0)
                {
                    // 注意：delete 命令如果找不到规则会返回错误，这里通常可以忽略
                }
            }
            catch (System.ComponentModel.Win32Exception)
            {
                throw new Exception("操作已取消：用户拒绝了管理员权限请求。");
            }
            catch (Exception ex)
            {
                throw new Exception($"系统配置执行失败: {ex.Message}");
            }
        }
    }
}