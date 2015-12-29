using Windows.UI.Xaml.Controls;
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
    }
}
