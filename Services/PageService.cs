using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Abstractions;

namespace DnDToolkit.Services
{
    /// <summary>
    /// 桥接服务：告诉 WPF-UI 如何通过依赖注入容器获取页面实例
    /// </summary>
    public class PageService : INavigationViewPageProvider
    {
        private readonly IServiceProvider _serviceProvider;

        // 注入 .NET 自带的 IServiceProvider
        public PageService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // 实现接口方法：根据类型获取页面实例
        public object? GetPage(Type pageType)
        {
            if (!typeof(FrameworkElement).IsAssignableFrom(pageType))
            {
                throw new InvalidOperationException($"The page {pageType.Name} should be a FrameworkElement.");
            }

            // 从 DI 容器中获取页面实例
            // 如果你在 App.xaml.cs 里注册了页面，这里就能取出来
            return _serviceProvider.GetService(pageType);
        }
    }
}
