using GalaSoft.MvvmLight.Views;
using msdnus.Shared;

namespace msdnus.ViewModel
{
    [Magic]
    public class CoffeeCupViewModel : PropertyChangedBase
    {
        public readonly INavigationService NavigationService;

        public CoffeeCupViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
        }
    }
}
