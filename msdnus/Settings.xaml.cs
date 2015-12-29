using Windows.UI.Xaml.Controls;
using msdnus.ViewModel;

namespace msdnus
{
    public sealed partial class Settings : Page
    {
        public SettingsViewModel Vm => (SettingsViewModel)DataContext;

        public Settings()
        {
            InitializeComponent();
        }
    }
}
