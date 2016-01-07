using System;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Store;
using GalaSoft.MvvmLight.Command;
using msdnus.Core.Model;
using msdnus.Shared;

namespace msdnus.ViewModel
{
    [Magic]
    public sealed class CoffeeCupViewModel : PropertyChangedBase
    {
        const string COFFE_CUP_IAP_NAME = "1 coffee cup";

        private readonly ISettingsProvider _settingsProvider;
        private readonly ResourceLoader _loader;

        private LicenseInformation _licenseInformation;

        public RelayCommand BuyCoffeeCommand { get; set; }

        public CoffeeCupViewModel(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
            _loader = ResourceLoader.GetForCurrentView();

            Init();
        }

        private void Init()
        {
            BuyCoffeeCommand = new RelayCommand(BuyFeatureCoffeeCup);
            try
            {
#if DEBUG
                _licenseInformation = CurrentAppSimulator.LicenseInformation;
#else
            _licenseInformation = CurrentApp.LicenseInformation;
#endif
                _settingsProvider.IsCoffeCuped = _licenseInformation.ProductLicenses[COFFE_CUP_IAP_NAME].IsActive;
            }
            catch
            {
                //ignorerd
            }
        }

        async void BuyFeatureCoffeeCup()
        {
            if (!_licenseInformation.ProductLicenses[COFFE_CUP_IAP_NAME].IsActive)
            {
                try
                {
                    await CurrentAppSimulator.RequestProductPurchaseAsync(COFFE_CUP_IAP_NAME, false);
                    //Check the license state to determine if the in-app purchase was successful.
                    _settingsProvider.IsCoffeCuped = _licenseInformation.ProductLicenses[COFFE_CUP_IAP_NAME].IsActive;
                    TextToastManager.Instance.ShowToast(_loader.GetString("purcok"));
                }
                catch (Exception)
                {
                    _settingsProvider.IsCoffeCuped = false;
                    TextToastManager.Instance.ShowToast(_loader.GetString("purcerror"));
                }
            }
            else
            {
                // The customer already owns this feature.
                _settingsProvider.IsCoffeCuped = true;
            }
        }
    }
}
