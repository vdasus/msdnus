using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using msdnus.Core.Model;
using msdnus.Core.Shared;
using msdnus.Services;
using msdnus.Shared;
using msdnus.ViewModel;
using System;
using System.IO;

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

            UnhandledException += (_, e) =>
            {
                LogCrash("XamlUnhandled", e.Exception);
                e.Handled = true;
            };
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
                LogCrash("AppDomainUnhandled", e.ExceptionObject as Exception);
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (_, e) =>
            {
                LogCrash("UnobservedTask", e.Exception);
                e.SetObserved();
            };

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Register Services
                    services.AddSingleton<ICryptoService, AesGcmCryptoService>();
                    services.AddSingleton<IDataSource, FileDataSource>();
                    services.AddSingleton<IDataTarget>(sp =>
                    {
                        var crypto = sp.GetRequiredService<ICryptoService>();
                        var dbPath = Path.Combine(AppEnvironment.DataFolder, "msdnus.litedb");
                        return new DataTarget(dbPath, crypto.DeriveSecret("msdnusdb"));
                    });
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
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
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
            MainWindow.Closed += (_, _) => _host.Dispose();
            MainWindow.Activate();
        }

        private void OnNavigationFailed(object sender, Microsoft.UI.Xaml.Navigation.NavigationFailedEventArgs e)
        {
            throw new Exception($"Failed to load page {e.SourcePageType.FullName}: {e.Exception}");
        }

        private static void LogCrash(string source, Exception? ex)
        {
            try
            {
                File.AppendAllText(
                    Path.Combine(AppEnvironment.DataFolder, "crash.log"),
                    $"{DateTime.Now:O} [{source}] {ex}{Environment.NewLine}");
            }
            catch
            {
                // logging must never crash the app
            }
        }
    }
}
