using GalaSoft.MvvmLight.Views;
using msdnus.Shared;

namespace msdnus.ViewModel
{
    [Magic]
    public class SettingsViewModel: PropertyChangedBase
    {
        public readonly INavigationService NavigationService;

        public SettingsViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
        }
    }
}
