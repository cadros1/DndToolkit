using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace DnDToolkit.Helpers
{
    public static class NetworkHelper
    {
        /// <summary>
        /// 获取本机在局域网中的 IPv4 地址
        /// </summary>
        public static string GetLocalIpAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());

                // 优先查找常见的局域网段 (192.168.x.x, 10.x.x.x, 172.x.x.x)
                var ip = host.AddressList.FirstOrDefault(ip =>
                    ip.AddressFamily == AddressFamily.InterNetwork &&
                    (ip.ToString().StartsWith("192.168.") ||
                     ip.ToString().StartsWith("10.") ||
                     ip.ToString().StartsWith("172.")));

                // 如果没找到典型局域网IP，则返回第一个非回环的IPv4
                if (ip == null)
                {
                    ip = host.AddressList.FirstOrDefault(i =>
                        i.AddressFamily == AddressFamily.InterNetwork &&
                        !IPAddress.IsLoopback(i));
                }

                return ip?.ToString() ?? "127.0.0.1";
            }
            catch
            {
                return "127.0.0.1";
            }
        }
    }
}