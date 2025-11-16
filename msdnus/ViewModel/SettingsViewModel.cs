using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using msdnus.Core.Model;
using msdnus.Shared;

namespace msdnus.ViewModel
{
    [Magic]
    public sealed partial class SettingsViewModel : ObservableObject
    {
        private readonly ISettingsProvider _settingsProvider;

        [ObservableProperty]
        private string iejimo1;

        [ObservableProperty]
        private string iejimo2;

        [ObservableProperty]
        private string statusErrText = string.Empty;

        public bool IsDarkTheme
        {
            get => _settingsProvider.IsDarkTheme;
            set
            {
                if (_settingsProvider.IsDarkTheme != value)
                {
                    _settingsProvider.IsDarkTheme = value;
                    OnPropertyChanged();
                    ApplyTheme();
                }
            }
        }

        public SettingsViewModel(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        [RelayCommand]
        private void SaveIejimoParam()
        {
            if (!string.IsNullOrEmpty(Iejimo1) && !string.IsNullOrEmpty(Iejimo2) && Iejimo1 == Iejimo2)
            {
                _settingsProvider.SetIejimoParameter(Iejimo1);
                ShowToast("Password saved successfully");
                StatusErrText = string.Empty;
                Iejimo1 = string.Empty;
                Iejimo2 = string.Empty;
            }
            else
            {
                StatusErrText = "Passwords do not match or are empty";
            }
        }

        private void ApplyTheme()
        {
            if (App.MainWindow?.Content is Microsoft.UI.Xaml.FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = IsDarkTheme 
                    ? Microsoft.UI.Xaml.ElementTheme.Dark 
                    : Microsoft.UI.Xaml.ElementTheme.Light;
            }
        }

        private void ShowToast(string message)
        {
            TextToastManager.Instance.ShowToast(message);
        }
    }
}
