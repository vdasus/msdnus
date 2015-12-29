using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using msdnus.ViewModel;

namespace msdnus
{
    public sealed partial class About : Page
    {
        public AboutViewModel Vm => (AboutViewModel)DataContext;

        public About()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Vm.NavigationService.GoBack();
        }
    }
}
