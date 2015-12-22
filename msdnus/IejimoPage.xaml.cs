using System;
using Windows.UI.Xaml;
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
            // Increment a counter each time the event fires.
            Vm?.CheckPassword?.Execute(passwordBox.Password);
        }
    }
}
