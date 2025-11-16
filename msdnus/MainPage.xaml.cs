using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using msdnus.ViewModel;

namespace msdnus
{
    public sealed partial class MainPage
    {
        private readonly bool _isregistered;

        public MainViewModel Vm => (MainViewModel)DataContext;

        public MainPage()
        {
            InitializeComponent();

            if (_isregistered) return;
            Splitter.RegisterPropertyChangedCallback(SplitView.IsPaneOpenProperty, IsPaneOpenPropertyChanged);
            Messenger.Default.Register<NotificationMessage>(this, NotificationMessageReceived);

            _isregistered = true;
        }

        private void NotificationMessageReceived(NotificationMessage obj)
        {
            if (obj.Notification == "Logout")
            {
                Vm.IsIejimoMode = true;
            }
        }

        private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            PasswordBox.Password = string.Empty;
            PasswordBox.Focus(FocusState.Programmatic);

        }

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (Vm.SearchStringChangedCommand.CanExecute(null))
                Vm.SearchStringChangedCommand.Execute(null);
        }

        private void PasswordChangedHandler(object sender, RoutedEventArgs e)
        {
            Vm?.CheckPassword?.Execute(PasswordBox.Password);
        }

        private void IsPaneOpenPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            PasswordBox.Password = string.Empty;
            PasswordBox.Focus(FocusState.Programmatic);
        }

        private void Splitter_OnPaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
            if(Vm.IsIejimoMode) args.Cancel = true;
        }
    }
}
