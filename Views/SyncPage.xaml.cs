using DnDToolkit.ViewModels;
using System.Windows.Controls;

namespace DnDToolkit.Views
{
    public partial class SyncPage : Page
    {
        public SyncPage(SyncViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}