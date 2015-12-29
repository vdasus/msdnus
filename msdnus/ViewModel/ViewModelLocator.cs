/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:Parkavimas" x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using msdnus.Core.Model;
using msdnus.Shared;
using Microsoft.Practices.ServiceLocation;

namespace msdnus.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            Lex.Db.Serialization.Extender.RegisterType<YourKeyKeyDTO, CustomTypesSerializers>(1000);

            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<IejimoViewModel>();
            SimpleIoc.Default.Register<AboutViewModel>();
            SimpleIoc.Default.Register<CoffeeCupViewModel>();
            SimpleIoc.Default.Register<HelpViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();

            SimpleIoc.Default.Register<IDataSource, FileDataSource>();
            SimpleIoc.Default.Register<IDataTarget, DataTarget>();
            SimpleIoc.Default.Register<ISettingsProvider, SettingsProvider>();

            var navigationService = CreateNavigationService();
            SimpleIoc.Default.Register<INavigationService>(() => navigationService);
        }

        public static MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
        public static IejimoViewModel Iejimo => ServiceLocator.Current.GetInstance<IejimoViewModel>();
        public static AboutViewModel About => ServiceLocator.Current.GetInstance<AboutViewModel>();
        public static CoffeeCupViewModel CoffeeCup => ServiceLocator.Current.GetInstance<CoffeeCupViewModel>();
        public static HelpViewModel Help => ServiceLocator.Current.GetInstance<HelpViewModel>();
        public static SettingsViewModel Settings => ServiceLocator.Current.GetInstance<SettingsViewModel>();

        private INavigationService CreateNavigationService()
        {
            var navigationService = new NavigationService();
            navigationService.Configure("Iejimo", typeof(IejimoPage));
            navigationService.Configure("Main", typeof(MainPage));
            navigationService.Configure("About", typeof(About));
            navigationService.Configure("CoffeeCup", typeof(CoffeeCup));
            navigationService.Configure("Help", typeof(Help));
            navigationService.Configure("Settings", typeof(Settings));
            
            return navigationService;
        }

        public static void Cleanup()
        {
            // TODO: Clear the ViewModels
        }
    }
}