using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace DnDToolkit.Helpers
{
    public static class ImageHelper
    {
        public static string BitmapImageToBase64(BitmapImage image)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                byte[] imageBytes = stream.ToArray();
                return Convert.ToBase64String(imageBytes);
            }
        }

        public static BitmapImage Base64ToBitmapImage(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);

            var image = new BitmapImage();
            using (var stream = new MemoryStream(imageBytes))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
            }

            image.Freeze(); // 跨线程使用
            return image;
        }

        // 从文件加载图像并转换为 Base64
        public static string? LoadImageFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            var bitmapImage = new BitmapImage(new Uri(filePath));
            return BitmapImageToBase64(bitmapImage);
        }
    }
}
