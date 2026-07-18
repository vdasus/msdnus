using System;
using System.IO;

namespace msdnus.Shared
{
    /// <summary>
    /// Abstracts away the difference between running with MSIX package identity
    /// (Store / sideloaded install) and running unpackaged (dev inner loop, CI).
    /// </summary>
    public static class AppEnvironment
    {
        public static bool IsPackaged { get; } = GetIsPackaged();

        /// <summary>Writable per-user data folder valid in both packaged and unpackaged mode.</summary>
        public static string DataFolder { get; } = GetDataFolder();

        private static bool GetIsPackaged()
        {
            try
            {
                _ = Windows.ApplicationModel.Package.Current;
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        private static string GetDataFolder()
        {
            if (IsPackaged)
            {
                return Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            }

            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "msdnus");
            Directory.CreateDirectory(folder);
            return folder;
        }
    }
}
