using System.Threading.Tasks;
using Windows.Storage;
using msdnus.Core.Model;

namespace msdnus.Shared
{
    public class SettingsProvider: ISettingsProvider
    {
        private static readonly ApplicationDataContainer Sett = ApplicationData.Current.RoamingSettings;
        public bool FirstRun
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

        static SettingsProvider()
        {
        }

        public async Task<string> GetIejimoParameter()
        {
            var value = Sett.Values["IejiPar"];
            return value == null ? "123" : await Core.Shared.Utils.UnprotectFromBase64Data((string) value);
        }

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
