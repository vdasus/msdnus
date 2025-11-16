using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using msdnus.ViewModel;

namespace msdnus
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel Vm { get; }

        public MainPage()
        {
            InitializeComponent();
            
            // Get ViewModel from DI
            Vm = App.Services.GetRequiredService<MainViewModel>();
            DataContext = Vm;
            
            Loaded += MainPage_OnLoaded;
            Splitter.RegisterPropertyChangedCallback(SplitView.IsPaneOpenProperty, IsPaneOpenPropertyChanged);
        }

        private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            PasswordBox.Password = string.Empty;
            PasswordBox.Focus(FocusState.Programmatic);
        }

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Vm?.SearchStringChangedCommand?.Execute(null);
        }

        private void PasswordChangedHandler(object sender, RoutedEventArgs e)
        {
            Vm?.CheckPasswordCommand?.Execute(PasswordBox.Password);
        }

        private void IsPaneOpenPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            PasswordBox.Password = string.Empty;
            PasswordBox.Focus(FocusState.Programmatic);
        }

        private void Splitter_OnPaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
            if (Vm.IsIejimoMode) 
                args.Cancel = true;
        }
    }
}
