using System;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using msdnus.Core.Model;
using msdnus.Shared;

namespace msdnus.ViewModel
{
    [Magic]
    public class IejimoViewModel: PropertyChangedBase
    {
        public readonly INavigationService NavigationService;
        private readonly ISettingsProvider _settingsProvider;

        public RelayCommand<string> CheckPassword { get; set; }

        private string _tiejimas = string.Empty;

        public IejimoViewModel(INavigationService navigationService, ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
            NavigationService = navigationService;

            Task.Run(() => Init(NavigationService));
        }

        private async Task<bool> Init(INavigationService navigationService)
        {
            if (navigationService == null) throw new ArgumentNullException(nameof(navigationService));
            try
            {
                _tiejimas = await _settingsProvider.GetIejimoParameter();
                CheckPassword = new RelayCommand<string>(e =>
                {
                    if (string.IsNullOrEmpty(_tiejimas) || e == _tiejimas)
                        NavigationService.NavigateTo("Main");
                });

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
