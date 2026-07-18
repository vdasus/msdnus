using System;

namespace msdnus.Shared
{
    /// <summary>
    /// Resource-string lookup with an English fallback, so the app still works
    /// when MRT resources are unavailable (e.g. some unpackaged scenarios).
    /// </summary>
    public static class Loc
    {
        private static readonly Lazy<Microsoft.Windows.ApplicationModel.Resources.ResourceLoader?> Loader =
            new(() =>
            {
                try
                {
                    return new Microsoft.Windows.ApplicationModel.Resources.ResourceLoader();
                }
                catch
                {
                    return null;
                }
            });

        public static string Get(string key, string fallback)
        {
            try
            {
                var value = Loader.Value?.GetString(key);
                return string.IsNullOrEmpty(value) ? fallback : value;
            }
            catch
            {
                return fallback;
            }
        }
    }
}
