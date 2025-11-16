using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using msdnus.ViewModel;

namespace msdnus
{
    public sealed partial class CoffeeCup : Page
    {
        public CoffeeCupViewModel Vm { get; }

        public CoffeeCup()
        {
            InitializeComponent();
            Vm = App.Services.GetRequiredService<CoffeeCupViewModel>();
            DataContext = Vm;
        }
    }
}
