using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using msdnus.ViewModel;

namespace msdnus
{
    public sealed partial class Help : Page
    {
        public HelpViewModel Vm { get; }

        public Help()
        {
            InitializeComponent();
            Vm = App.Services.GetRequiredService<HelpViewModel>();
            DataContext = Vm;
        }
    }
}
