using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly VaultService _vault;
        private readonly IPublicClientApplication _msalClient;

        private CancellationTokenSource? _infoCts;
        private bool _suppressProfileChange;

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

        [ObservableProperty]
        public partial string? SelectedProfileName { get; set; }

        public ObservableCollection<string> Profiles { get; } = new();

        public bool IsUnlocked => !IsLocked;

        /// <summary>True on first run (or after a reset): the user must create a PIN instead of entering one.</summary>
        public bool NeedsPinSetup => !_vault.HasPin;

        public bool HasPin => _vault.HasPin;

        public bool HasProducts => Products.Count > 0;

        /// <summary>Empty state is shown only when the database is empty (not for empty search results).</summary>
        public bool ShowEmptyState => Products.Count == 0 && string.IsNullOrWhiteSpace(SearchText) && !IsBusy;

        public string ResultsSummary =>
            Products.Count == 0 ? string.Empty : string.Format(Loc.Get("ResultsFmt", "{0} products"), Products.Count);

        public bool IsDarkTheme
        {
            get => _vault.Settings.IsDarkTheme;
            set
            {
                if (_vault.Settings.IsDarkTheme == value) return;
                _vault.SaveTheme(value);
                OnPropertyChanged();
                ApplyTheme();
            }
        }

        public MainViewModel(IDataSource datasource, VaultService vault)
        {
            _datasource = datasource;
            _vault = vault;

            _msalClient = PublicClientApplicationBuilder
                .Create("000000004017CE14")
                .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
                .WithAuthority("https://login.microsoftonline.com/consumers")
                .Build();

            IsLocked = true;
            NotifyIfLegacyDatabasePresent();
        }

        partial void OnIsLockedChanged(bool value) => OnPropertyChanged(nameof(IsUnlocked));

        partial void OnProductsChanged(List<YourKeyDTO> value)
        {
            OnPropertyChanged(nameof(HasProducts));
            OnPropertyChanged(nameof(ShowEmptyState));
            OnPropertyChanged(nameof(ResultsSummary));
        }

        partial void OnSearchTextChanged(string? value) => _ = RefreshAsync();

        partial void OnSelectedProfileNameChanged(string? value)
        {
            if (_suppressProfileChange || string.IsNullOrEmpty(value)) return;
            _vault.SelectProfile(value);
            _ = RefreshAsync();
        }

        // ---- Unlock / PIN --------------------------------------------------

        /// <summary>Unlocks an existing vault with the entered PIN.</summary>
        public void SubmitPin(string pin)
        {
            if (!_vault.TryUnlock(pin))
            {
                ShowInfo(Loc.Get("MsgWrongPin", "Wrong PIN"), InfoBarSeverity.Error);
                return;
            }

            OnUnlocked();
        }

        /// <summary>Creates the first PIN (first run / after reset) and unlocks. Returns false when the PINs do not match.</summary>
        public bool CreatePin(string pin1, string pin2)
        {
            if (string.IsNullOrEmpty(pin1) || pin1 != pin2)
            {
                ShowInfo(Loc.Get("MsgPinMismatch", "PINs are empty or do not match"), InfoBarSeverity.Warning);
                return false;
            }

            _vault.CreatePin(pin1);
            OnPropertyChanged(nameof(NeedsPinSetup));
            OnPropertyChanged(nameof(HasPin));
            OnUnlocked();
            return true;
        }

        /// <summary>Called from the settings dialog. Re-encrypts all profiles with the new PIN.</summary>
        public bool TryChangePin(string pin1, string pin2)
        {
            if (string.IsNullOrEmpty(pin1) || pin1 != pin2)
            {
                ShowInfo(Loc.Get("MsgPinMismatch", "PINs are empty or do not match"), InfoBarSeverity.Warning);
                return false;
            }

            if (!_vault.ChangePin(pin1))
            {
                ShowInfo(Loc.Get("MsgPinChangeFailed", "Could not change the PIN"), InfoBarSeverity.Error);
                return false;
            }

            ShowInfo(Loc.Get("MsgPinSaved", "PIN changed, everything re-encrypted"), InfoBarSeverity.Success);
            return true;
        }

        private void OnUnlocked()
        {
            IsLocked = false;
            RefreshProfiles();
            _ = RefreshAsync();
        }

        [RelayCommand]
        private void Lock()
        {
            SearchText = string.Empty;
            _vault.Lock();
            IsLocked = true;
        }

        [RelayCommand]
        private async Task ResetPin()
        {
            var xamlRoot = App.MainWindow?.Content?.XamlRoot;
            if (xamlRoot == null) return;

            var dialog = new ContentDialog
            {
                Title = Loc.Get("DlgResetTitle", "Reset everything"),
                Content = Loc.Get("DlgResetText",
                    "Forgot the PIN? All profiles and imported keys will be permanently erased and you will set a new PIN."),
                PrimaryButtonText = Loc.Get("DlgOk", "Reset"),
                CloseButtonText = Loc.Get("DlgCancel", "Cancel"),
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = xamlRoot
            };

            if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;

            _vault.ResetAll();
            IsLocked = true;
            Products = new List<YourKeyDTO>();
            RefreshProfiles();
            OnPropertyChanged(nameof(NeedsPinSetup));
            OnPropertyChanged(nameof(HasPin));
            ShowInfo(Loc.Get("MsgVaultReset", "Vault reset — create a new PIN"), InfoBarSeverity.Informational);
        }

        // ---- Profiles ------------------------------------------------------

        [RelayCommand]
        private async Task AddProfile()
        {
            var xamlRoot = App.MainWindow?.Content?.XamlRoot;
            if (xamlRoot == null) return;

            var input = new TextBox { PlaceholderText = Loc.Get("ProfileNameHint", "Profile name") };
            var dialog = new ContentDialog
            {
                Title = Loc.Get("DlgAddProfileTitle", "New profile"),
                Content = input,
                PrimaryButtonText = Loc.Get("DlgCreate", "Create"),
                CloseButtonText = Loc.Get("DlgCancel", "Cancel"),
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = xamlRoot
            };

            if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;

            if (!_vault.AddProfile(input.Text))
            {
                ShowInfo(Loc.Get("MsgProfileInvalid", "Empty name or a profile with that name already exists"),
                    InfoBarSeverity.Warning);
                return;
            }

            SearchText = string.Empty;
            RefreshProfiles();
            await RefreshAsync();
        }

        [RelayCommand]
        private async Task DeleteProfile()
        {
            var name = _vault.SelectedProfile;
            if (string.IsNullOrEmpty(name)) return;

            if (_vault.ProfileNames.Count <= 1)
            {
                ShowInfo(Loc.Get("MsgLastProfile", "At least one profile is required"), InfoBarSeverity.Warning);
                return;
            }

            var xamlRoot = App.MainWindow?.Content?.XamlRoot;
            if (xamlRoot == null) return;

            var dialog = new ContentDialog
            {
                Title = Loc.Get("DlgDeleteProfileTitle", "Delete profile"),
                Content = string.Format(
                    Loc.Get("DlgDeleteProfileText", "Delete profile \"{0}\" and all its keys?"), name),
                PrimaryButtonText = Loc.Get("DlgDelete", "Delete"),
                CloseButtonText = Loc.Get("DlgCancel", "Cancel"),
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = xamlRoot
            };

            if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;

            _vault.RemoveProfile(name);
            SearchText = string.Empty;
            RefreshProfiles();
            await RefreshAsync();
        }

        private void RefreshProfiles()
        {
            _suppressProfileChange = true;
            Profiles.Clear();
            foreach (var n in _vault.ProfileNames) Profiles.Add(n);
            SelectedProfileName = _vault.SelectedProfile;
            _suppressProfileChange = false;
        }

        // ---- Data ----------------------------------------------------------

        private async Task RefreshAsync()
        {
            var target = _vault.Current;
            if (target == null)
            {
                Products = new List<YourKeyDTO>();
                return;
            }

            try
            {
                Products = await target.GetDataValueByNameAsync(SearchText ?? string.Empty);
            }
            catch (Exception ex)
            {
                ShowInfo(ex.Message, InfoBarSeverity.Error);
            }
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
            var target = _vault.Current;
            if (target == null) return false;

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

            return target.SaveData(list, iscleanbefore: true, isdistinct: true);
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
