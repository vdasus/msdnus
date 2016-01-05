using System.Threading.Tasks;
using Windows.Storage;
using msdnus.Core.Model;

namespace msdnus.Shared
{
    public class SettingsProvider : ISettingsProvider
    {
        private static readonly ApplicationDataContainer Sett = ApplicationData.Current.RoamingSettings;

        public bool IsFirstRun
        {
            get
            {
                var value = Sett.Values["FirstRun"];
                if (value == null)
                {
                    Sett.Values["FirstRun"] = true;
                }
                return (bool)Sett.Values["FirstRun"];
            }

            set { Sett.Values["FirstRun"] = value; }
        }

        public bool IsDarkTheme
        {
            get
            {
                var value = Sett.Values["IsDarkTheme"];
                if (value == null)
                {
                    Sett.Values["IsDarkTheme"] = false;
                }
                return (bool)Sett.Values["IsDarkTheme"];
            }

            set { Sett.Values["IsDarkTheme"] = value; }
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
            var value = Sett.Values["IejiPar"];

            if (value == null)
            {
                await SetIejimoParameter("123");
                value = Sett.Values["IejiPar"];
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
                Sett.Values["IejiPar"] = await Core.Shared.Utils.ProtectToBase64Async(astr);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

}
