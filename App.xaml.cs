using DnDToolkit.Services;
using DnDToolkit.ViewModels;
using DnDToolkit.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.Data;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace DnDToolkit
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static IHost _host;
        public static IServiceProvider ServiceProvider => _host.Services;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // --- WPF-UI v4 核心服务注册 ---

                    // 1. 注册 PageService，将其作为 INavigationViewPageProvider 的实现
                    services.AddSingleton<INavigationViewPageProvider, PageService>();

                    // 2. 注册 NavigationService
                    services.AddSingleton<INavigationService, NavigationService>();

                    // 3. 其他 UI 服务
                    services.AddSingleton<ISnackbarService, SnackbarService>();
                    services.AddSingleton<IContentDialogService, ContentDialogService>();

                    // --- 你的业务服务 ---
                    services.AddSingleton<ICharacterService, CharacterService>();
                    services.AddSingleton<PdfDataService>();
                    services.AddSingleton<ResourceService>();
                    services.AddSingleton<UpdateService>();
                    services.AddSingleton<LanSyncService>();

                    // --- ViewModels ---
                    services.AddSingleton<MainWindowViewModel>();
                    services.AddSingleton<CharacterListViewModel>();
                    services.AddSingleton<EditorViewModel>();
                    services.AddSingleton<AdventureViewModel>();
                    services.AddSingleton<MoreViewModel>();
                    services.AddSingleton<ResourcesViewModel>();
                    services.AddSingleton<AboutViewModel>();
                    services.AddSingleton<SyncViewModel>();

                    // --- Windows & Pages ---
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<CharacterListPage>();
                    services.AddSingleton<EditorPage>();
                    services.AddSingleton<AdventurePage>();
                    services.AddSingleton<MorePage>();
                    services.AddSingleton<ResourcesPage>();
                    services.AddSingleton<AboutPage>();
                    services.AddSingleton<SyncPage>();
                })
                .Build();

            await _host.StartAsync();

            try
            {
                //await PdfService.ExtractFieldNamesToTxtAsync("D:\\output.txt");
                var mainWindow = _host.Services.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                var logPath = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "error.log"
                );
                var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 启动失败: {ex}\n";
                System.IO.File.AppendAllText(logPath, logMessage);

                System.Windows.MessageBox.Show($"启动失败: {ex.Message}");
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            base.OnExit(e);
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // 这会让错误弹窗显示，而不是直接崩溃无声无息
            System.Windows.MessageBox.Show($"未捕获异常: {e.Exception.Message}\n\n堆栈: {e.Exception.StackTrace}", "程序崩溃");
            e.Handled = true; // 防止程序立即退出
        }
    }
}
