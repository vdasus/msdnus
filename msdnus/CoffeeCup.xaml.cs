using Windows.UI.Xaml.Controls;
using msdnus.ViewModel;

namespace msdnus
{
    public sealed partial class CoffeeCup : Page
    {
        public CoffeeCupViewModel Vm => (CoffeeCupViewModel)DataContext;

        public CoffeeCup()
        {
            InitializeComponent();
        }
    }
}
