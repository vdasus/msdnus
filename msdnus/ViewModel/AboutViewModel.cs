using CommunityToolkit.Mvvm.ComponentModel;
using Windows.ApplicationModel;
using msdnus.Shared;

namespace msdnus.ViewModel
{
    [Magic]
    public sealed partial class AboutViewModel : ObservableObject
    {
        public string AppVersion { get; } = GetAppVersion();

        private static string GetAppVersion()
        {
            try
            {
                var version = Package.Current.Id.Version;
                return $"v.{version.Major}.{version.Minor}.{version.Build}";
            }
            catch
            {
                return "v.1.0.0";
            }
        }
    }
}
