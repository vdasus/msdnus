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

        public bool IsCoffeCuped
        {
            get
            {
                var value = _sett.Values["IsCoffeCuped"];
                if (value == null)
                {
                    _sett.Values["IsCoffeCuped"] = false;
                }
                return (bool)_sett.Values["IsCoffeCuped"];
            }

            set { _sett.Values["IsCoffeCuped"] = value; }
        }

        static SettingsProvider()
        {
        }

        public string GetIejimoParameterAsync()
        {
            try
            {
                var value = _sett.Values["IejiPar"];

                if (value == null)
                {
                    SetIejimoParameter("123");
                    value = _sett.Values["IejiPar"];
                }

                return Core.Shared.Utils.UnprotectFromBase64DataAsync((string) value);
            }
            catch
            {
                return null;
            }
        }

        public bool SetIejimoParameter(string astr)
        {
            try
            {
                var tmp = Core.Shared.Utils.ProtectToBase64Async(astr);
                if (tmp == null) return false;
                _sett.Values["IejiPar"] = tmp;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
