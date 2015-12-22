using System;
using Windows.UI.Xaml;
using msdnus.ViewModel;

namespace msdnus
{
    public sealed partial class IejimoPage
    {
        public IejimoViewModel IVm => (IejimoViewModel)DataContext;

        public IejimoPage()
        {
            InitializeComponent();
        }

        void PasswordChangedHandler(Object sender, RoutedEventArgs args)
        {
            IVm?.CheckPassword?.Execute(passwordBox.Password);
        }

        private void IejimoPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            passwordBox.Focus(FocusState.Programmatic);
        }
    }
}
