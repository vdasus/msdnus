using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using msdnus.ViewModel;

namespace msdnus
{
    public sealed partial class Settings : Page
    {
        public SettingsViewModel Vm { get; }

        public Settings()
        {
            InitializeComponent();
            Vm = App.Services.GetRequiredService<SettingsViewModel>();
            DataContext = Vm;
        }

        private void PasswordChangedHandler(object sender, RoutedEventArgs e)
        {
            Vm.Iejimo1 = Iejimo1.Password;
            Vm.Iejimo2 = Iejimo2.Password;
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            // ViewModel handles theme switching directly
            Vm.IsDarkTheme = IsDarkTheme.IsOn;
        }
    }
}
