using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using msdnus.Core.Model;
using msdnus.Core.Shared;
using msdnus.Shared;
using msdnus.ViewModel;
using System;
using System.IO;

namespace msdnus
{
    public partial class App : Application
    {
        private readonly IHost _host;
        public static Window? MainWindow { get; private set; }
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
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton<IDataSource, FileDataSource>();
                    services.AddSingleton<AppSettingsStore>();
                    services.AddSingleton<VaultService>();
                    services.AddSingleton<MainViewModel>();
                })
                .Build();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            MainWindow = new Window
            {
                Title = "msdnus — MSDN Product Keys",
                SystemBackdrop = new MicaBackdrop()
            };

            var page = new MainPage();
            MainWindow.Content = page;

            // Apply the saved theme before the window becomes visible
            var settings = Services.GetRequiredService<AppSettingsStore>();
            page.RequestedTheme = settings.Settings.IsDarkTheme ? ElementTheme.Dark : ElementTheme.Light;

            MainWindow.ExtendsContentIntoTitleBar = true;
            MainWindow.SetTitleBar(page.FindName("AppTitleBar") as UIElement);

            MainWindow.Closed += (_, _) => _host.Dispose();
            MainWindow.Activate();
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
