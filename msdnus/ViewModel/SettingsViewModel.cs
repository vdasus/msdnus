using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using msdnus.Core.Model;
using msdnus.Shared;

namespace msdnus.ViewModel
{
    [Magic]
    public class SettingsViewModel: PropertyChangedBase
    {
        public readonly INavigationService NavigationService;
        private readonly ISettingsProvider _settingsProvider;

        public string IejimoParametras { get; set; }

        public bool IsDarkTheme
        {
            get { return _settingsProvider.IsDarkTheme; }
            set { _settingsProvider.IsDarkTheme = value; }
        }

        public RelayCommand SaveIejimoParamCommand { get; set; }
        
        
        public SettingsViewModel(INavigationService navigationService, ISettingsProvider settingsProvider)
        {
            NavigationService = navigationService;
            _settingsProvider = settingsProvider;

            SaveIejimoParamCommand = new RelayCommand(() =>
            {
                
            });
        }
    }
}
