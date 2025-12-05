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
    /// CharacterListPage.xaml 的交互逻辑
    /// </summary>
    public partial class CharacterListPage : Page
    {
        public CharacterListPage(CharacterListViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // 每次进入页面时加载数据
            this.Loaded += async (s, e) => await viewModel.LoadDataAsync();
        }
    }
}
