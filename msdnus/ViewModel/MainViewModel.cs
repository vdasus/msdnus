using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using msdnus.Core.Model;
using msdnus.Core.Shared;
using msdnus.Services;
using msdnus.Shared;
using Microsoft.Identity.Client;
using WinRT.Interop;

namespace msdnus.ViewModel
{
    public sealed partial class MainViewModel : ObservableObject
    {
        private readonly IDataSource _datasource;
        private readonly IDataTarget _datatarget;
        private readonly ISettingsProvider _settingsProvider;
        private readonly INavigationService _navigationService;
        private readonly IPublicClientApplication _msalClient;

        [ObservableProperty]
        public partial NotifyTaskCompletion<List<YourKeyDTO>>? DataItems { get; set; }

        [ObservableProperty]
        public partial string? TextToSearch { get; set; }

        [ObservableProperty]
        public partial string? StatusText { get; set; }

        [ObservableProperty]
        public partial Visibility IsProgress { get; set; }

        [ObservableProperty]
        public partial bool IsIejimoMode { get; set; }

        public MainViewModel(
            IDataSource datasource, 
            IDataTarget datatarget, 
            ISettingsProvider settingsProvider,
            INavigationService navigationService)
        {
            _datasource = datasource;
            _datatarget = datatarget;
            _settingsProvider = settingsProvider;
            _navigationService = navigationService;

            // Initialize MSAL for Microsoft Graph
            _msalClient = PublicClientApplicationBuilder
                .Create("000000004017CE14")
                .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
                .WithAuthority("https://login.microsoftonline.com/consumers")
                .Build();

            Init();
        }

        private void Init()
        {
            SetProgress(true);

            try
            {
                if (_settingsProvider.IsFirstRun)
                {
                    ResetIejimoPwd();
                    _settingsProvider.IsFirstRun = false;
                }

                // Set theme
                try
                {
                    var window = App.MainWindow;
                    if (window?.Content is FrameworkElement rootElement)
                    {
                        rootElement.RequestedTheme = _settingsProvider.IsDarkTheme 
                            ? ElementTheme.Dark 
                            : ElementTheme.Light;
                    }
                }
                catch
                {
                    // ignored
                }

                LockAll();
                NotifyIfLegacyDatabasePresent();
                DataItems = new NotifyTaskCompletion<List<YourKeyDTO>>(_datatarget.GetAllDataAsync());
            }
            catch (Exception ex)
            {
#if DEBUG
                ShowToast($"msdnus:ERR:Init:{ex.StackTrace}");
#else
                ShowToast($"msdnus:{ex.Message}");
#endif
            }
            finally
            {
                SetProgress(false);
            }
        }

        [RelayCommand]
        private void CheckPassword(string password)
        {
            var storedPassword = _settingsProvider.GetIejimoParameterAsync();

            if (string.IsNullOrEmpty(storedPassword) || password != storedPassword) 
                return;

            IsIejimoMode = false;
        }

        [RelayCommand]
        private void CopyToClipboard(string text)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(text);
            Clipboard.SetContent(dataPackage);
            ShowToast("Text copied to clipboard");
        }

        [RelayCommand]
        private void SearchStringChanged()
        {
            DataItems = new NotifyTaskCompletion<List<YourKeyDTO>>(
                _datatarget.GetDataValueByNameAsync(TextToSearch ?? string.Empty));
        }

        [RelayCommand]
        private async Task GetFromFilePicker()
        {
            try
            {
                var openPicker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.Thumbnail,
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                };
                openPicker.FileTypeFilter.Add(".xml");

                // Initialize the file picker with the window handle (WinUI 3)
                var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
                InitializeWithWindow.Initialize(openPicker, hwnd);

                StorageFile file = await openPicker.PickSingleFileAsync();

                if (file == null) return;

                SetProgress(true);

                if (!await ImportDataAsync(file, true, true)) return;

                DataItems = new NotifyTaskCompletion<List<YourKeyDTO>>(_datatarget.GetAllDataAsync());
                ShowToast("Data loaded and imported successfully");
            }
            catch (Exception ex)
            {
                ShowToast($"msdnus:Err:{ex.Message}");
            }
            finally
            {
                SetProgress(false);
            }
        }

        [RelayCommand]
        private void LockAll()
        {
            IsIejimoMode = true;
        }

        [RelayCommand]
        private async Task LoadFromOneDrive()
        {
            try
            {
                SetProgress(true);

                // Authenticate with Microsoft Graph
                var scopes = new[] { "Files.Read", "User.Read" };
                AuthenticationResult authResult;

                try
                {
                    var accounts = await _msalClient.GetAccountsAsync();
                    authResult = await _msalClient.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                        .ExecuteAsync();
                }
                catch (MsalUiRequiredException)
                {
                    authResult = await _msalClient.AcquireTokenInteractive(scopes)
                        .WithParentActivityOrWindow(WindowNative.GetWindowHandle(App.MainWindow))
                        .ExecuteAsync();
                }

                // Download the file from OneDrive via the Microsoft Graph REST endpoint
                using var http = new System.Net.Http.HttpClient();
                http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult.AccessToken);

                using var response = await http.GetAsync(
                    "https://graph.microsoft.com/v1.0/me/drive/root:/KeysExport.xml:/content");
                response.EnsureSuccessStatusCode();

                using (var memoryStream = new MemoryStream())
                {
                    await response.Content.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    if (!ImportData(memoryStream, true, true)) return;
                }

                DataItems = new NotifyTaskCompletion<List<YourKeyDTO>>(_datatarget.GetAllDataAsync());
                ShowToast("Data loaded from OneDrive successfully");
            }
            catch (Exception ex)
            {
                ShowToast($"msdnus:Err:{ex.Message}");
            }
            finally
            {
                SetProgress(false);
            }
        }

        [RelayCommand]
        private void NavigateToAbout()
        {
            _navigationService.NavigateTo("About");
        }

        [RelayCommand]
        private void NavigateToCoffeeCup()
        {
            _navigationService.NavigateTo("CoffeeCup");
        }

        [RelayCommand]
        private void NavigateToHelp()
        {
            _navigationService.NavigateTo("Help");
        }

        [RelayCommand]
        private void NavigateToSettings()
        {
            _navigationService.NavigateTo("Settings");
        }

        [RelayCommand]
        private async Task ResetIejimoPwd()
        {
            var dialog = new ContentDialog
            {
                Title = "Reset Password",
                Content = "Are you sure you want to reset the password?",
                PrimaryButtonText = "Ok",
                CloseButtonText = "Cancel",
                XamlRoot = App.MainWindow.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result != ContentDialogResult.Primary) return;

            await _datatarget.CleanDbAsync();
            _settingsProvider.SetIejimoParameter("123");
            DataItems = new NotifyTaskCompletion<List<YourKeyDTO>>(_datatarget.GetAllDataAsync());
            ShowToast("Password reset successfully");
        }

        partial void OnTextToSearchChanged(string? value)
        {
            SearchStringChanged();
        }

        private async Task<bool> ImportDataAsync(StorageFile file, bool isCleanupBefore, bool isDistinct)
        {
            using var stream = await file.OpenStreamForReadAsync();
            return ImportData(stream, isCleanupBefore, isDistinct);
        }

        private bool ImportData(Stream stream, bool isCleanupBefore, bool isDistinct)
        {
            root data = _datasource.GetData(stream);
            return SaveData(data, isCleanupBefore, isDistinct);
        }

        private bool SaveData(root data, bool isCleanupBefore, bool isDistinct)
        {
            List<YourKeyDTO> list = new List<YourKeyDTO>();
            var yourKey = data.Items?.OfType<rootYourKey>().FirstOrDefault();

            if (yourKey?.Product_Key == null) return false;

            foreach (rootYourKeyProduct_Key productKey in yourKey.Product_Key)
            {
                YourKeyDTO dto = new YourKeyDTO();
                dto.MapFromYourKeyProduct_Key(productKey);
                list.Add(dto);
            }

            return _datatarget.SaveData(list, isCleanupBefore, isDistinct);
        }

        private void NotifyIfLegacyDatabasePresent()
        {
            try
            {
                // The pre-migration app stored data in a Lex.Db folder named "msdnusdb".
                // That format (and its encryption) is not supported anymore — ask for a re-import.
                var legacyDbPath = Path.Combine(AppEnvironment.DataFolder, "msdnusdb");
                if (Directory.Exists(legacyDbPath))
                {
                    ShowToast("Data from a previous version was found",
                        "Please re-import your keys XML file (the old database format is no longer supported).");
                }
            }
            catch
            {
                // ignored — informational only
            }
        }

        private void SetProgress(bool isShow)
        {
            if (isShow)
            {
                StatusText = "Loading...";
                IsProgress = Visibility.Visible;
            }
            else
            {
                StatusText = string.Empty;
                IsProgress = Visibility.Collapsed;
            }
        }

        private void ShowToast(string message, string? details = null)
        {
            TextToastManager.Instance.ShowToast(message, details);
        }
    }
}
