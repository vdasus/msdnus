using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using msdnus.ViewModel;

namespace msdnus
{
    public sealed partial class IejimoPage
    {
        public IejimoViewModel Vm => (IejimoViewModel)DataContext;

        public IejimoPage()
        {
            InitializeComponent();
        }

        void PasswordChangedHandler(Object sender, RoutedEventArgs args)
        {
            Vm?.CheckPassword?.Execute(passwordBox.Password);
        }

        private void IejimoPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            passwordBox.Focus(FocusState.Programmatic);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Vm.NavigationService.GoBack();
        }
    }
}
