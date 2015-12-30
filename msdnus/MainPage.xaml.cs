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
            Splitter.RegisterPropertyChangedCallback(SplitView.IsPaneOpenProperty, IsPaneOpenPropertyChanged);
        }

        private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            passwordBox.Password = string.Empty;
            passwordBox.Focus(FocusState.Programmatic);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Vm.NavigationService.GoBack();
        }

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (Vm.SearchStringChangedCommand.CanExecute(null)) Vm.SearchStringChangedCommand.Execute(null);
        }

        private void PasswordChangedHandler(object sender, RoutedEventArgs e)
        {
            Vm?.CheckPassword?.Execute(passwordBox.Password);
        }

        private void IsPaneOpenPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            passwordBox.Password = string.Empty;
            passwordBox.Focus(FocusState.Programmatic);
        }

        private void Splitter_OnPaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
            if(Vm.IsIejimoMode) args.Cancel = true;
        }
    }
}
