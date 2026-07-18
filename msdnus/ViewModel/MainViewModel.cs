using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using msdnus.Core.Model;
using msdnus.Shared;
using Microsoft.Identity.Client;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace msdnus.ViewModel
{
    public sealed partial class MainViewModel : ObservableObject
    {
        private readonly IDataSource _datasource;
        private readonly IDataTarget _datatarget;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IPublicClientApplication _msalClient;

        private CancellationTokenSource? _infoCts;

        [ObservableProperty]
        public partial List<YourKeyDTO> Products { get; set; } = new();

        [ObservableProperty]
        public partial string? SearchText { get; set; }

        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        public partial bool IsLocked { get; set; }

        [ObservableProperty]
        public partial bool IsInfoOpen { get; set; }

        [ObservableProperty]
        public partial string InfoMessage { get; set; } = string.Empty;

        [ObservableProperty]
        public partial InfoBarSeverity InfoSeverity { get; set; } = InfoBarSeverity.Success;

        public bool IsUnlocked => !IsLocked;

        public bool HasProducts => Products.Count > 0;

        /// <summary>Empty state is shown only when the database is empty (not for empty search results).</summary>
        public bool ShowEmptyState => Products.Count == 0 && string.IsNullOrWhiteSpace(SearchText) && !IsBusy;

        public string ResultsSummary =>
            Products.Count == 0 ? string.Empty : string.Format(Loc.Get("ResultsFmt", "{0} products"), Products.Count);

        public bool IsDarkTheme
        {
            get => _settingsProvider.IsDarkTheme;
            set
            {
                if (_settingsProvider.IsDarkTheme == value) return;
                _settingsProvider.IsDarkTheme = value;
                OnPropertyChanged();
                ApplyTheme();
            }
        }

        public MainViewModel(
            IDataSource datasource,
            IDataTarget datatarget,
            ISettingsProvider settingsProvider)
        {
            _datasource = datasource;
            _datatarget = datatarget;
            _settingsProvider = settingsProvider;

            _msalClient = PublicClientApplicationBuilder
                .Create("000000004017CE14")
                .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
                .WithAuthority("https://login.microsoftonline.com/consumers")
                .Build();

            if (_settingsProvider.IsFirstRun)
            {
                _settingsProvider.SetIejimoParameter("123");
                _settingsProvider.IsFirstRun = false;
            }

            IsLocked = true;
            NotifyIfLegacyDatabasePresent();
            _ = RefreshAsync();
        }

        partial void OnIsLockedChanged(bool value) => OnPropertyChanged(nameof(IsUnlocked));

        partial void OnProductsChanged(List<YourKeyDTO> value)
        {
            OnPropertyChanged(nameof(HasProducts));
            OnPropertyChanged(nameof(ShowEmptyState));
            OnPropertyChanged(nameof(ResultsSummary));
        }

        partial void OnSearchTextChanged(string? value) => _ = RefreshAsync();

        private async Task RefreshAsync()
        {
            try
            {
                Products = await _datatarget.GetDataValueByNameAsync(SearchText ?? string.Empty);
            }
            catch (Exception ex)
            {
                ShowInfo(ex.Message, InfoBarSeverity.Error);
            }
        }

        [RelayCommand]
        private void CheckPassword(string password)
        {
            var storedPassword = _settingsProvider.GetIejimoParameterAsync();

            if (string.IsNullOrEmpty(storedPassword) || password != storedPassword)
                return;

            IsLocked = false;
        }

        [RelayCommand]
        private void Lock()
        {
            SearchText = string.Empty;
            IsLocked = true;
        }

        [RelayCommand]
        private void CopyKey(YourKeyKeyDTO key)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(key.Value);
            Clipboard.SetContent(dataPackage);
            ShowInfo(Loc.Get("MsgCopied", "Key copied to clipboard"), InfoBarSeverity.Success);
        }

        [RelayCommand]
        private async Task ImportFromFile()
        {
            try
            {
                var openPicker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.List,
                    SuggestedStartLocation = PickerLocationId.Downloads
                };
                openPicker.FileTypeFilter.Add(".xml");
                InitializeWithWindow.Initialize(openPicker, WindowNative.GetWindowHandle(App.MainWindow));

                StorageFile file = await openPicker.PickSingleFileAsync();
                if (file == null) return;

                IsBusy = true;

                using var stream = await file.OpenStreamForReadAsync();
                await FinishImportAsync(ImportData(stream));
            }
            catch (Exception ex)
            {
                ShowInfo($"{Loc.Get("MsgImportErr", "Import failed")}: {ex.Message}", InfoBarSeverity.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ImportFromOneDrive()
        {
            try
            {
                IsBusy = true;

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

                using var http = new System.Net.Http.HttpClient();
                http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult.AccessToken);

                using var response = await http.GetAsync(
                    "https://graph.microsoft.com/v1.0/me/drive/root:/KeysExport.xml:/content");
                response.EnsureSuccessStatusCode();

                using var memoryStream = new MemoryStream();
                await response.Content.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                await FinishImportAsync(ImportData(memoryStream));
            }
            catch (Exception ex)
            {
                ShowInfo($"{Loc.Get("MsgImportErr", "Import failed")}: {ex.Message}", InfoBarSeverity.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ResetPin()
        {
            var xamlRoot = App.MainWindow?.Content?.XamlRoot;
            if (xamlRoot == null) return;

            var dialog = new ContentDialog
            {
                Title = Loc.Get("DlgResetTitle", "Reset PIN"),
                Content = Loc.Get("DlgResetText", "Reset the PIN to \"123\"? All imported keys will be erased."),
                PrimaryButtonText = Loc.Get("DlgOk", "Reset"),
                CloseButtonText = Loc.Get("DlgCancel", "Cancel"),
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = xamlRoot
            };

            if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;

            await _datatarget.CleanDbAsync();
            _settingsProvider.SetIejimoParameter("123");
            await RefreshAsync();
            ShowInfo(Loc.Get("MsgPinReset", "PIN reset to \"123\", data erased"), InfoBarSeverity.Informational);
        }

        /// <summary>Called from the settings dialog. Returns false when the PINs do not match.</summary>
        public bool TryChangePin(string pin1, string pin2)
        {
            if (string.IsNullOrEmpty(pin1) || pin1 != pin2)
            {
                ShowInfo(Loc.Get("MsgPinMismatch", "PINs are empty or do not match"), InfoBarSeverity.Warning);
                return false;
            }

            _settingsProvider.SetIejimoParameter(pin1);
            ShowInfo(Loc.Get("MsgPinSaved", "PIN changed"), InfoBarSeverity.Success);
            return true;
        }

        private async Task FinishImportAsync(bool imported)
        {
            if (!imported)
            {
                ShowInfo(Loc.Get("MsgImportBad", "The file does not look like an MSDN keys export"), InfoBarSeverity.Warning);
                return;
            }

            SearchText = string.Empty;
            await RefreshAsync();
            ShowInfo(string.Format(Loc.Get("MsgImportOk", "Imported {0} products"), Products.Count), InfoBarSeverity.Success);
        }

        private bool ImportData(Stream stream)
        {
            root data = _datasource.GetData(stream);
            var yourKey = data.Items?.OfType<rootYourKey>().FirstOrDefault();

            if (yourKey?.Product_Key == null) return false;

            var list = new List<YourKeyDTO>();
            foreach (rootYourKeyProduct_Key productKey in yourKey.Product_Key)
            {
                var dto = new YourKeyDTO();
                dto.MapFromYourKeyProduct_Key(productKey);
                list.Add(dto);
            }

            return _datatarget.SaveData(list, iscleanbefore: true, isdistinct: true);
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
                    ShowInfo(Loc.Get("MsgLegacyDb",
                        "Data from a previous version was found — please re-import your keys XML."),
                        InfoBarSeverity.Informational, autoHide: false);
                }
            }
            catch
            {
                // ignored — informational only
            }
        }

        private void ShowInfo(string message, InfoBarSeverity severity, bool autoHide = true)
        {
            _infoCts?.Cancel();
            InfoMessage = message;
            InfoSeverity = severity;
            IsInfoOpen = true;

            if (!autoHide) return;

            var cts = new CancellationTokenSource();
            _infoCts = cts;
            _ = HideLaterAsync(cts);
        }

        private async Task HideLaterAsync(CancellationTokenSource cts)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(3), cts.Token);
                IsInfoOpen = false;
            }
            catch (TaskCanceledException)
            {
                // a newer message replaced this one
            }
        }

        private void ApplyTheme()
        {
            if (App.MainWindow?.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = IsDarkTheme ? ElementTheme.Dark : ElementTheme.Light;
            }
        }
    }
}
