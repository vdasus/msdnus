using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using msdnus.ViewModel;

namespace msdnus
{
    public sealed partial class About : Page
    {
        public AboutViewModel Vm { get; }

        public About()
        {
            InitializeComponent();
            Vm = App.Services.GetRequiredService<AboutViewModel>();
            DataContext = Vm;
        }
    }
}
