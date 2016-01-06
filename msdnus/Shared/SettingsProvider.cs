using System.Threading.Tasks;
using Windows.Storage;
using msdnus.Core.Model;

namespace msdnus.Shared
{
    public class SettingsProvider : ISettingsProvider
    {
        private static readonly ApplicationDataContainer _sett = ApplicationData.Current.RoamingSettings;

        public bool IsFirstRun
        {
            get
            {
                var value = _sett.Values["IsFirstRun"];
                if (value == null)
                {
                    _sett.Values["IsFirstRun"] = true;
                }
                return (bool)_sett.Values["IsFirstRun"];
            }

            set { _sett.Values["IsFirstRun"] = value; }
        }

        public bool IsDarkTheme
        {
            get
            {
                var value = _sett.Values["IsDarkTheme"];
                if (value == null)
                {
                    _sett.Values["IsDarkTheme"] = false;
                }
                return (bool)_sett.Values["IsDarkTheme"];
            }

            set { _sett.Values["IsDarkTheme"] = value; }
        }

        static SettingsProvider()
        {
        }

        /// <summary>
        /// Get iejimo parameter
        /// </summary>
        /// <returns>null if unsuccessful</returns>
        public async Task<string> GetIejimoParameter()
        {
            var value = _sett.Values["IejiPar"];

            if (value == null)
            {
                await SetIejimoParameter("123");
                value = _sett.Values["IejiPar"];
            }

            try
            {
                return await Core.Shared.Utils.UnprotectFromBase64Data((string)value);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Set Iejimo parameter
        /// </summary>
        /// <param name="astr">string to set</param>
        /// <returns>true if successful</returns>
        public async Task<bool> SetIejimoParameter(string astr)
        {
            try
            {
                _sett.Values["IejiPar"] = await Core.Shared.Utils.ProtectToBase64Async(astr);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

}
