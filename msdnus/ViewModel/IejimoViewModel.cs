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
        private readonly INavigationService _navigationService;
        private readonly ISettingsProvider _settingsProvider;
        public RelayCommand<string> CheckPassword { get; set; }

        private string tiejimas = string.Empty;

        public IejimoViewModel(INavigationService navigationService, ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
            _navigationService = navigationService;

            Task.Run(() => Init(_navigationService));
        }

        private bool Init(INavigationService navigationService)
        {
            try
            {
                //TODO temporary
                //tiejimas = await _settingsProvider.GetIejimoParameter();
                tiejimas = "123";
                CheckPassword = new RelayCommand<string>(e =>
                {
                    if (string.IsNullOrEmpty(tiejimas) || e == tiejimas)
                        _navigationService.NavigateTo("Main");
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
