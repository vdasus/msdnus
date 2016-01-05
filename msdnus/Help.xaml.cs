using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using msdnus.ViewModel;

//https://msdn.microsoft.com/en-us/subscriptions/keys/

namespace msdnus
{
    public sealed partial class Help : Page
    {
        public HelpViewModel Vm => (HelpViewModel)DataContext;

        public Help()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Vm.NavigationService.GoBack();
        }
    }
}
