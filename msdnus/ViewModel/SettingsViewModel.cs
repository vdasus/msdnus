using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using msdnus.Shared;

namespace msdnus.ViewModel
{
    [Magic]
    public class SettingsViewModel: PropertyChangedBase
    {
        public readonly INavigationService NavigationService;

        public string IejimoParametras { get; set; }
        public RelayCommand SaveIejimoParamCommand { get; set; }
        
        
        public SettingsViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;

            SaveIejimoParamCommand = new RelayCommand(() =>
            {
                
            });
        }
    }
}
