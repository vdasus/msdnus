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
        private readonly IIejimoProvider _iejimoProvider;
        public RelayCommand<string> CheckPassword { get; set; }


        public IejimoViewModel(INavigationService navigationService, IIejimoProvider iejimoProvider)
        {
            _navigationService = navigationService;
            _iejimoProvider = iejimoProvider;

            CheckPassword = new RelayCommand<string>(e =>
            {
                var tmp = _iejimoProvider.GetIejimoParametra();
                if (string.IsNullOrEmpty(tmp) || e == tmp)
                    navigationService.NavigateTo("Main");
            });
        }
    }
}
