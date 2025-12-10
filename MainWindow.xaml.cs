using DnDToolkit.ViewModels;
using DnDToolkit.Views;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace DnDToolkit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        public MainWindow(
            MainWindowViewModel viewModel,
            INavigationService navigationService,
            ISnackbarService snackbarService
            )
        {
            InitializeComponent();
            DataContext = viewModel;

            // --- WPF-UI v4 修正 ---

            // 1. 设置 NavigationService 的控制对象
            navigationService.SetNavigationControl(RootNavigation);

            // 注意：v4 不需要 RootNavigation.SetPageService(...) 
            // 也不需要 RootNavigation.SetServiceProvider(...)
            // 这些工作现在由 INavigationViewPageProvider 在幕后通过 NavigationService 处理

            // 2. 导航到首页
            Loaded += (_, _) => navigationService.Navigate(typeof(CharacterListPage));

            snackbarService.SetSnackbarPresenter(SnackbarPresenter);
        }
    }
}