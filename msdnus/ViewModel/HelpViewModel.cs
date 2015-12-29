using GalaSoft.MvvmLight.Views;
using msdnus.Shared;

namespace msdnus.ViewModel
{
    [Magic]
    public class HelpViewModel : PropertyChangedBase
    {
        public readonly INavigationService NavigationService;

        public HelpViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
        }
    }
}
