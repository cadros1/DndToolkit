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
    /// EditorPage.xaml 的交互逻辑
    /// </summary>
    public partial class EditorPage : Page // 注意这里如果 using Wpf.Ui.Controls 可能会冲突，显式继承 System.Windows.Controls.Page 或 Wpf.Ui.Controls.UiPage
    {
        // 推荐继承 System.Windows.Controls.Page 配合 WPF-UI 样式
        public EditorPage(EditorViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
