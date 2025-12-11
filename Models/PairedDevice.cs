using System;

namespace DnDToolkit.Models
{
    public class PairedDevice
    {
        public string DeviceId { get; set; } = "";   // 手机的唯一标识 (UUID/AndroidID)
        public string DeviceName { get; set; } = ""; // 例如 "Pixel 7"
        public DateTime LastConnected { get; set; }  // 最后连接时间
        public bool IsTrusted { get; set; } = true;  // 是否已信任（配对）
    }
}