using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui;

namespace DnDToolkit.ViewModels
{
    public partial class MoreViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;

        public MoreViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        [RelayCommand]
        private void GoToResources()
        {
            // 跳转到资源页
            _navigationService.Navigate(typeof(Views.ResourcesPage));
        }

        [RelayCommand]
        private void GoToAbout()
        {
            _navigationService.Navigate(typeof(Views.AboutPage));
        }
    }
}