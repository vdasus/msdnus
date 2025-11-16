using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using msdnus.Core.Model;
using msdnus.Services;
using msdnus.Shared;
using msdnus.ViewModel;
using System;

namespace msdnus
{
    public partial class App : Application
    {
        private IHost _host;
        public static Window MainWindow { get; private set; }
        public static IServiceProvider Services => ((App)Current)._host.Services;

        public App()
        {
            InitializeComponent();

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Register Services
                    services.AddSingleton<IDataSource, FileDataSource>();
                    services.AddSingleton<IDataTarget, DataTarget>();
                    services.AddSingleton<ISettingsProvider, SettingsProvider>();
                    services.AddSingleton<INavigationService, NavigationService>();

                    // Register ViewModels
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<SettingsViewModel>();
                    services.AddSingleton<AboutViewModel>();
                    services.AddSingleton<HelpViewModel>();
                    services.AddSingleton<CoffeeCupViewModel>();

                    // Register Pages
                    services.AddTransient<MainPage>();
                    services.AddTransient<Settings>();
                    services.AddTransient<About>();
                    services.AddTransient<Help>();
                    services.AddTransient<CoffeeCup>();
                })
                .Build();

            // Initialize Lex.Db serialization
            Lex.Db.Serialization.Extender.RegisterType<YourKeyKeyDTO, CustomTypesSerializers>(1000);
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            MainWindow = new Window
            {
                Title = "msdnus - Password Manager"
            };

            var frame = new Frame();
            frame.NavigationFailed += OnNavigationFailed;

            // Configure navigation service
            var navigationService = Services.GetRequiredService<INavigationService>() as NavigationService;
            if (navigationService != null)
            {
                navigationService.Frame = frame;
                navigationService.Configure("Main", typeof(MainPage));
                navigationService.Configure("Settings", typeof(Settings));
                navigationService.Configure("About", typeof(About));
                navigationService.Configure("Help", typeof(Help));
                navigationService.Configure("CoffeeCup", typeof(CoffeeCup));
            }

            // Navigate to main page
            frame.Navigate(typeof(MainPage), e.Arguments);

            MainWindow.Content = frame;
            MainWindow.Activate();
        }

        private void OnNavigationFailed(object sender, Microsoft.UI.Xaml.Navigation.NavigationFailedEventArgs e)
        {
            throw new Exception($"Failed to load page {e.SourcePageType.FullName}: {e.Exception}");
        }
    }
}
