using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;

namespace DnDToolkit.Helpers
{
    public class Base64ToImageConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? base64 = value as string;
            if (string.IsNullOrEmpty(base64)) return null;

            try
            {
                byte[] binaryData = System.Convert.FromBase64String(base64);
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = new MemoryStream(binaryData);
                bi.EndInit();
                return bi;
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                // 检查是否有 "Inverse" 参数
                bool isInverse = parameter is string str && str.Equals("Inverse", StringComparison.OrdinalIgnoreCase);

                if (isInverse)
                {
                    // 反转逻辑：是 0 就显示，不是 0 就隐藏
                    // (用于：当 Count == 0 时显示"暂无角色")
                    return intValue == 0 ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    // 默认逻辑：是 0 就隐藏，不是 0 就显示
                    // (用于：当 Level == 0 时隐藏法术位输入框)
                    return intValue == 0 ? Visibility.Collapsed : Visibility.Visible;
                }
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = value as string;
            bool hasText = !string.IsNullOrEmpty(str);

            // 检查是否有参数 "Inverse"。如果有，逻辑反转
            // 用于占位符：没有文本时显示 (Visible)
            if (parameter is string paramStr && paramStr.Equals("Inverse", StringComparison.OrdinalIgnoreCase))
            {
                return hasText ? Visibility.Collapsed : Visibility.Visible;
            }

            // 默认逻辑：有文本时显示 (Visible)
            return hasText ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
