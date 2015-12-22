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
            passwordBox.Focus(FocusState.Programmatic);
        }

        void PasswordChangedHandler(Object sender, RoutedEventArgs args)
        {
            IVm?.CheckPassword?.Execute(passwordBox.Password);
        }
    }
}
