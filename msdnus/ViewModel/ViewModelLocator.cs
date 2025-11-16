/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:msdnus" x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using msdnus.Core.Model;
using msdnus.Shared;

namespace msdnus.ViewModel
{
    public sealed class ViewModelLocator
    {
        static ViewModelLocator()
        {
            Lex.Db.Serialization.Extender.RegisterType<YourKeyKeyDTO, CustomTypesSerializers>(1000);

            Ioc.Default.ConfigureServices(
                new ServiceCollection()
                    .AddSingleton<IDataSource, FileDataSource>()
                    .AddSingleton<IDataTarget, DataTarget>()
                    .AddSingleton<ISettingsProvider, SettingsProvider>()
                    .AddSingleton<HelpViewModel>()
                    .AddSingleton<SettingsViewModel>()
                    .AddSingleton<AboutViewModel>()
                    .AddSingleton<CoffeeCupViewModel>()
                    .AddSingleton<MainViewModel>()
                    .BuildServiceProvider()
            );
        }

        public HelpViewModel Help => Ioc.Default.GetService<HelpViewModel>();
        public SettingsViewModel Settings => Ioc.Default.GetService<SettingsViewModel>();
        public AboutViewModel About => Ioc.Default.GetService<AboutViewModel>();
        public CoffeeCupViewModel CoffeeCup => Ioc.Default.GetService<CoffeeCupViewModel>();
        public MainViewModel Main => Ioc.Default.GetService<MainViewModel>();

        public void Cleanup()
        {
            // TODO: Clear the ViewModels if needed
        }
    }
}