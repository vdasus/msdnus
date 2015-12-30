using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using msdnus.ViewModel;

namespace msdnus
{
    public sealed partial class MainPage
    {
        public MainViewModel Vm => (MainViewModel)DataContext;

        public MainPage()
        {
            InitializeComponent();
        }

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if(Vm.SearchStringChangedCommand.CanExecute(null)) Vm.SearchStringChangedCommand.Execute(null);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Vm.NavigationService.GoBack();
        }

        private void PasswordChangedHandler(object sender, RoutedEventArgs e)
        {
            Vm?.CheckPassword?.Execute(passwordBox.Password);
        }

        private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            passwordBox.Focus(FocusState.Programmatic);
        }
    }
}
