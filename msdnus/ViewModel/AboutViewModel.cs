using Windows.ApplicationModel;
using msdnus.Shared;

namespace msdnus.ViewModel
{
    [Magic]
    public class AboutViewModel: PropertyChangedBase
    {
        public string AppVersion { get; } =
    $"v.{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";
    }
}
