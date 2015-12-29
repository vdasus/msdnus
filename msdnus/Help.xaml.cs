using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using msdnus.ViewModel;

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
