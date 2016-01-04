using Windows.UI.Xaml;
using msdnus.Shared;
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

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            var themeAwareFrame = Frame as ThemeAwareFrame;
            if (themeAwareFrame == null) return;

            if (IsDarkTheme.IsOn)
            {
                themeAwareFrame.AppTheme = ElementTheme.Dark;
                Vm.IsDarkTheme = true;
            }
            else
            {
                themeAwareFrame.AppTheme = ElementTheme.Light;
                Vm.IsDarkTheme = false;
            }
        }
    }
}
