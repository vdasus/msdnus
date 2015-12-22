using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using msdnus.Shared;

namespace msdnus.ViewModel
{
    [Magic]
    public class IejimoViewModel: PropertyChangedBase
    {
        private readonly INavigationService _navigationService;

        public RelayCommand<string> CheckPassword { get; set; }


        public IejimoViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            CheckPassword = new RelayCommand<string>(e =>
            {
                if(e == "123") navigationService.NavigateTo("Main");
            });
        }
    }
}
