using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnDToolkit.Services;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace DnDToolkit.ViewModels
{
    public partial class ResourcesViewModel : ObservableObject
    {
        private readonly ResourceService _resourceService;
        private readonly ISnackbarService _snackbarService;

        public ResourcesViewModel(ResourceService resourceService, ISnackbarService snackbarService)
        {
            _resourceService = resourceService;
            _snackbarService = snackbarService;
        }

        [RelayCommand]
        private async Task DownloadFile(string fileName)
        {
            // 1. 弹出保存对话框
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = fileName,
                Filter = "All Files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // 2. 提取资源
                    await _resourceService.ExtractResourceToFileAsync(fileName, saveFileDialog.FileName);

                    // 3. 成功提示
                    _snackbarService.Show(
                        "下载成功",
                        $"{fileName} 已保存。",
                        ControlAppearance.Success,
                        new SymbolIcon(SymbolRegular.Checkmark24),
                        TimeSpan.FromSeconds(3));
                }
                catch (Exception ex)
                {
                    _snackbarService.Show("下载失败", ex.Message, ControlAppearance.Danger);
                }
            }
        }

        [RelayCommand]
        private void OpenLink(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true // 关键：必须为 true 才能打开浏览器
                });
            }
            catch (Exception ex)
            {
                _snackbarService.Show("错误", $"无法打开链接: {ex.Message}", ControlAppearance.Danger);
            }
        }
    }
}