using msdnus.ViewModel;

namespace msdnus
{
    public sealed partial class About
    {
        public AboutViewModel Vm => (AboutViewModel)DataContext;

        public About()
        {
            InitializeComponent();
        }
    }
}
