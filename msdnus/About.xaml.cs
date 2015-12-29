using Windows.UI.Xaml.Controls;
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
    }
}
