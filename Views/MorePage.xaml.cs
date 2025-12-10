using DnDToolkit.ViewModels;
using System.Windows.Controls;

namespace DnDToolkit.Views
{
    public partial class MorePage : Page
    {
        public MorePage(MoreViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}