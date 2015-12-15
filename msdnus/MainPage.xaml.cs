using Windows.UI.Xaml.Controls;
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
            InitializeComponent();
        }

        private async void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if(Vm.SearchStringChanged.CanExecute(null)) Vm.SearchStringChanged.Execute(null);
        }
    }
}
