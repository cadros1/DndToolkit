using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnDToolkit.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string applicationTitle = "DnD Toolkit";

        // 如果你需要通过代码动态控制菜单项，可以在这里定义
        // [ObservableProperty]
        // private ObservableCollection<object> menuItems = new();

        public MainWindowViewModel()
        {
            // 初始化逻辑
        }
    }
}
