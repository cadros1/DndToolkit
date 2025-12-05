using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Input;

namespace DnDToolkit.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _welcomeMessage = "欢迎使用DnD Toolkit！";

        [ObservableProperty]
        private bool _isLoading;

        [RelayCommand(CanExecute = nameof(CanDownloadPdf))]
        private async Task DownloadPdfAsync()
        {
            IsLoading = true;

            try
            {
                // 模拟异步下载过程
                await Task.Delay(2000);

                // 在实际应用中，这里可能是：
                // 1. 打开文件对话框选择保存位置
                // 2. 从服务器下载 PDF 文件
                // 3. 生成并保存 PDF 文件

                MessageBox.Show("角色卡 PDF 下载完成！", "下载成功",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // 可选：打开下载的文件或文件夹
                // Process.Start("下载的文件路径");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"下载失败：{ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanDownloadPdf() => !IsLoading;
    }
}
