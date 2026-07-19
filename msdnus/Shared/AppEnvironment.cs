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
            // Packaged (MSIX) builds cannot write next to the executable (read-only install
            // location), so they keep using the per-user LocalFolder.
            // Unpackaged builds are fully portable: the database, settings and everything else
            // live in the same directory as the .exe, so the whole folder can be synced (e.g.
            // via Dropbox) and carried between machines.
            string folder;
            if (IsPackaged)
            {
                folder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            }
            else
            {
                folder = Path.GetDirectoryName(Environment.ProcessPath)
                         ?? AppContext.BaseDirectory;
            }

            Directory.CreateDirectory(folder);
            return folder;
        }
    }
}
