using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnDToolkit.Models;
using DnDToolkit.Services;
using System;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace DnDToolkit.ViewModels
{
    public partial class AboutViewModel : ObservableObject
    {
        private readonly UpdateService _updateService;
        private readonly ISnackbarService _snackbarService;

        [ObservableProperty] private string? currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        [ObservableProperty] private string updateStatus = "尚未检查";
        [ObservableProperty] private bool isChecking = false;
        [ObservableProperty] private bool hasNewVersion = false;
        [ObservableProperty] private string latestVersion = "";
        [ObservableProperty] private string releaseNotes = "";

        private string _downloadUrl = "";

        public AboutViewModel(UpdateService updateService, ISnackbarService snackbarService)
        {
            _updateService = updateService;
            _snackbarService = snackbarService;

            // 初始化显示版本
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            CurrentVersion = $"v{version?.Major}.{version?.Minor}.{version?.Build}";
        }

        [RelayCommand]
        private async Task CheckUpdate()
        {
            IsChecking = true;
            UpdateStatus = "正在连接 GitHub...";
            HasNewVersion = false;

            var result = await _updateService.CheckForUpdateAsync();

            IsChecking = false;
            // 刷新显示的当前版本（以此为准）
            CurrentVersion = $"v{result.currentVersion}";

            if (result.releaseInfo == null)
            {
                UpdateStatus = "检查失败 (网络错误或 API 限制)";
                _snackbarService.Show("错误", "无法连接到 GitHub API", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.ErrorCircle24), TimeSpan.FromSeconds(3));
                return;
            }

            if (result.hasUpdate)
            {
                HasNewVersion = true;
                LatestVersion = result.releaseInfo.TagName;
                ReleaseNotes = result.releaseInfo.Body;
                _downloadUrl = result.releaseInfo.HtmlUrl;
                UpdateStatus = "发现新版本！";

                _snackbarService.Show("发现更新", $"最新版本 {result.releaseInfo.TagName} 可用", ControlAppearance.Info, new SymbolIcon(SymbolRegular.ArrowDownload24), TimeSpan.FromSeconds(3));
            }
            else
            {
                UpdateStatus = "当前已是最新版本";
                _snackbarService.Show("检查完成", "暂无更新", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Checkmark24), TimeSpan.FromSeconds(2));
            }
        }

        [RelayCommand]
        private void GoToDownload()
        {
            if (!string.IsNullOrEmpty(_downloadUrl))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = _downloadUrl,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    UpdateStatus = $"无法打开链接: {ex.Message}";
                }
            }
        }
    }
}