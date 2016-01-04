using Windows.UI.Xaml;
using msdnus.ViewModel;

namespace msdnus
{
    public sealed partial class Settings
    {
        public SettingsViewModel Vm => (SettingsViewModel)DataContext;

        public Settings()
        {
            InitializeComponent();
        }

        private void PasswordChangedHandler(object sender, RoutedEventArgs e)
        {
            //TODO implement
        }
    }
}
