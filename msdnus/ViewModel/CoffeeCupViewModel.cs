using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using msdnus.Core.Model;
using msdnus.Shared;
using Microsoft.UI.Xaml.Controls;

namespace msdnus.ViewModel
{
    [Magic]
    public sealed partial class CoffeeCupViewModel : ObservableObject
    {
        const string COFFEE_CUP_IAP_NAME = "1 coffee cup";

        private readonly ISettingsProvider _settingsProvider;

        [ObservableProperty]
        private bool isCoffeeCuped;

        public CoffeeCupViewModel(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
            IsCoffeeCuped = _settingsProvider.IsCoffeCuped;
        }

        [RelayCommand]
        private async Task BuyCoffee()
        {
            try
            {
                // Show dialog for donation/support
                var dialog = new ContentDialog
                {
                    Title = "Support Development",
                    Content = "Thank you for considering supporting this application! " +
                             "In-app purchases are being updated for Windows App SDK. " +
                             "Please visit our website or GitHub to support development.",
                    PrimaryButtonText = "OK",
                    XamlRoot = App.MainWindow.Content.XamlRoot
                };

                await dialog.ShowAsync();

                // For now, mark as supported
                IsCoffeeCuped = true;
                _settingsProvider.IsCoffeCuped = true;
                ShowToast("Thank you for your support!");
            }
            catch (Exception ex)
            {
                ShowToast($"Error: {ex.Message}");
            }
        }

        private void ShowToast(string message)
        {
            TextToastManager.Instance.ShowToast(message);
        }
    }
}
