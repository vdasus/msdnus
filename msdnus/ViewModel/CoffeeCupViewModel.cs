using System;
using Windows.ApplicationModel.Store;
using GalaSoft.MvvmLight.Command;
using msdnus.Shared;

namespace msdnus.ViewModel
{
    [Magic]
    public class CoffeeCupViewModel : PropertyChangedBase
    {
        private readonly LicenseInformation _licenseInformation;
        public RelayCommand BuyCoffeeCommand { get; set; }

        public CoffeeCupViewModel()
        {
#if DEBUG
            _licenseInformation = CurrentAppSimulator.LicenseInformation;
#else
            licenseInformation = CurrentApp.LicenseInformation;
#endif

            BuyCoffeeCommand = new RelayCommand(BuyFeatureCoffeeCup);

            if (_licenseInformation.ProductLicenses["1 coffee cup"].IsActive)
            {
                // the customer can access this feature
            }
            else
            {
                // the customer can't access this feature
            }
        }

        async void BuyFeatureCoffeeCup()
        {
            if (!_licenseInformation.ProductLicenses["1 coffee cup"].IsActive)
            {
                try
                {
                    // The customer doesn't own this feature, so 
                    // show the purchase dialog.

                    await CurrentAppSimulator.RequestProductPurchaseAsync("1 coffee cup", false);
                    //Check the license state to determine if the in-app purchase was successful.
                }
                catch (Exception)
                {
                    // The in-app purchase was not completed because 
                    // an error occurred.
                }
            }
            else
            {
                // The customer already owns this feature.
            }
        }
    }
}
