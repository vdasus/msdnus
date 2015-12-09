using msdnus.ViewModel;

namespace msdnus
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        public MainViewModel Vm => (MainViewModel)DataContext;

        public MainPage()
        {
            this.InitializeComponent();
        }
    }
}
