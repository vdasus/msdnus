using Windows.ApplicationModel;
using GalaSoft.MvvmLight;
using msdnus.Shared;

namespace msdnus.ViewModel
{
    [Magic]
    public class MainViewModel: ViewModelBase
    {
        public string AppVersion { get; } =
    $"v.{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";


    }
}
