using DnDToolkit.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DnDToolkit.Views
{
    /// <summary>
    /// AdventurePage.xaml 的交互逻辑
    /// </summary>
    public partial class AdventurePage : Page
    {
        public AdventurePage(AdventureViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // 注册 Loaded 事件
            this.Loaded += AdventurePage_Loaded;
        }

        private async void AdventurePage_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is AdventureViewModel vm)
            {
                await vm.RefreshDataAsync();
            }
        }
    }
}
