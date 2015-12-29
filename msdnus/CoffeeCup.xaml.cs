using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using msdnus.ViewModel;

namespace msdnus
{
    public sealed partial class CoffeeCup : Page
    {
        public CoffeeCupViewModel Vm => (CoffeeCupViewModel)DataContext;

        public CoffeeCup()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Vm.NavigationService.GoBack();
        }
    }
}
