using msdnus.ViewModel;

namespace msdnus
{
    public sealed partial class CoffeeCup
    {
        public CoffeeCupViewModel Vm => (CoffeeCupViewModel)DataContext;

        public CoffeeCup()
        {
            InitializeComponent();
        }
    }
}
