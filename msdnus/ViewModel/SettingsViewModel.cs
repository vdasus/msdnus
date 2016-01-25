using Windows.ApplicationModel.Resources;
using GalaSoft.MvvmLight.Command;
using msdnus.Core.Model;
using msdnus.Shared;

namespace msdnus.ViewModel
{
    [Magic]
    public sealed class SettingsViewModel: PropertyChangedBase
    {
        private readonly ISettingsProvider _settingsProvider;
        private ResourceLoader _loader;

        public string Iejimo1 { get; set; }
        public string Iejimo2 { get; set; }
        
        public bool IsDarkTheme
        {
            get { return _settingsProvider.IsDarkTheme; }
            set { _settingsProvider.IsDarkTheme = value; }
        }

        public string StatusErrText { get; set; }
        
        public RelayCommand SaveIejimoParamCommand { get; set; }
        
        
        public SettingsViewModel(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;

            Init();
        }

        private void Init()
        {
            _loader = ResourceLoader.GetForCurrentView();

            StatusErrText = string.Empty;

            SaveIejimoParamCommand = new RelayCommand(() =>
            {
                if (!string.IsNullOrEmpty(Iejimo1) && !string.IsNullOrEmpty(Iejimo2) && (Iejimo1 == Iejimo2))
                {
                    _settingsProvider.SetIejimoParameterAsync(Iejimo1);
                    TextToastManager.Instance.ShowToast(_loader.GetString("iejsaved"));
                }
                else StatusErrText = _loader.GetString("iejerror");
            });
        }
    }
}
