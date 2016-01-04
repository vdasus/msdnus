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

            var navigationService = CreateNavigationService();
            SimpleIoc.Default.Register(() => navigationService);

            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<IDataSource, FileDataSource>();
            SimpleIoc.Default.Register<IDataTarget, DataTarget>();
            SimpleIoc.Default.Register<ISettingsProvider, SettingsProvider>();

            SimpleIoc.Default.Register<HelpViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<AboutViewModel>();
            SimpleIoc.Default.Register<CoffeeCupViewModel>();
            SimpleIoc.Default.Register<MainViewModel>();
        }

        public HelpViewModel Help => ServiceLocator.Current.GetInstance<HelpViewModel>();
        public SettingsViewModel Settings => ServiceLocator.Current.GetInstance<SettingsViewModel>();
        public AboutViewModel About => ServiceLocator.Current.GetInstance<AboutViewModel>();
        public CoffeeCupViewModel CoffeeCup => ServiceLocator.Current.GetInstance<CoffeeCupViewModel>();
        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
        
        private INavigationService CreateNavigationService()
        {
            var navigationService = new NavigationService();
            navigationService.Configure("Help", typeof(Help));
            navigationService.Configure("Settings", typeof(Settings));
            navigationService.Configure("About", typeof(About));
            navigationService.Configure("CoffeeCup", typeof(CoffeeCup));
            navigationService.Configure("Main", typeof(MainPage));

            return navigationService;
        }

        public void Cleanup()
        {
            // TODO: Clear the ViewModels
        }
    }
}