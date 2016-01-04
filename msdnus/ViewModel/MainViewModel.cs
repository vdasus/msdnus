using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using msdnus.Core.Model;
using msdnus.Shared;
using Microsoft.OneDrive.Sdk;

// ReSharper disable ExplicitCallerInfoArgument

namespace msdnus.ViewModel
{
    [Magic]
    public class MainViewModel: PropertyChangedBase
    {
        private readonly ResourceLoader _loader;

        public readonly INavigationService NavigationService;
        private readonly ISettingsProvider _settingsProvider;

        public List<YourKeyDTO> DataItems { get; set; }

        public string TextToSearch { get; set; }
        public string StatusText { get; set; }
        public Visibility IsProgress { get; set; }
        public bool IsIejimoMode { get; set; }

        #region commands
        public RelayCommand<string> CheckPassword { get; set; }
        public RelayCommand<string> CopyToClipboardCommand { get; set; }
        public RelayCommand SearchStringChangedCommand { get; set; }
        public RelayCommand GetFromFilePickerCommand { get; set; }
        public RelayCommand LockAllCommand { get; set; }
        public RelayCommand OnedriveCommand { get; set; }
        public RelayCommand AboutPageCommand { get; set; }
        public RelayCommand CoffeeCupCommand { get; set; }
        public RelayCommand HelpCommand { get; set; }
        public RelayCommand SettingsCommand { get; set; }
        #endregion

        private readonly IDataSource _datasource;
        private readonly IDataTarget _datatarget;

        //public ITaskCompletionNotifier Initialization { get; private set; }

        public MainViewModel(IDataSource datasource, IDataTarget datatarget, INavigationService navigationService, ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
            NavigationService = navigationService;
            
            _datasource = datasource;
            _datatarget = datatarget;
            
            _loader = ResourceLoader.GetForCurrentView();

            IsIejimoMode = true;

            Application.Current.RequestedTheme = !_settingsProvider.IsDarkTheme
                ? ApplicationTheme.Dark
                : ApplicationTheme.Light;

            Init();
        }

        private async void Init()
        {
            SetProgress(true);
            
            try
            {
                #region commands

                //CheckPassword
                CheckPassword = new RelayCommand<string>(async e =>
                {
                    var tiejimas = await _settingsProvider.GetIejimoParameter();
                    if (string.IsNullOrEmpty(tiejimas) || e == tiejimas)
                        IsIejimoMode = false;
                    RaisePropertyChanged("IsIejimoMode");
                });

                //CopyToClipboard
                CopyToClipboardCommand = new RelayCommand<string>(e =>
                {
                    /*MessageDialog msg = new MessageDialog(e);
                    await msg.ShowAsync();*/
                    DataPackage dp = new DataPackage();
                    dp.SetText(e);
                    Clipboard.SetContent(dp);
                    TextToastManager.Instance.ShowToast(_loader.GetString("copyedtext"));
                });

                //SearchStringChanged
                SearchStringChangedCommand = new RelayCommand(async () =>
                {
                    DataItems = await _datatarget.GetDataValueByName(TextToSearch ?? string.Empty);
                    RaisePropertyChanged("DataItems");
                });

                //GetFromFilePickerCommand
                GetFromFilePickerCommand = new RelayCommand(async () =>
                {
                    try
                    {
                        if (!EnsureUnsnapped()) return;

                        FileOpenPicker openPicker = new FileOpenPicker
                        {
                            ViewMode = PickerViewMode.Thumbnail,
                            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                        };
                        openPicker.FileTypeFilter.Add(".xml");

                        StorageFile file = await openPicker.PickSingleFileAsync();

                        if (file == null) return;

                        SetProgress(true);

                        if (!await ImportData(file, true, true)) return;

                        DataItems = await _datatarget.GetAllData();
                        RaisePropertyChanged("DataItems");
                        TextToastManager.Instance.ShowToast(_loader.GetString("loadednimported"));
                    }
                    catch (Exception ex)
                    {
                        TextToastManager.Instance.ShowToast($"msdnus:Err:{ex.Message}");
                    }
                    finally
                    {
                        SetProgress(false);
                    }
                });

                //LockAll
                LockAllCommand = new RelayCommand(() =>
                {
                    IsIejimoMode = true;
                });

                //OnedriveCommand
                OnedriveCommand = new RelayCommand(async () =>
                {
                    //ClID: 000000004017CE14
                    //ClSc: 3Iq0D38qYplvEhpoLGpVpidVXpiEdf1X
                    try
                    {
                        SetProgress(true);

                        if (!EnsureUnsnapped()) return;

                        var oneDriveClient =
                            OneDriveClientExtensions.GetClientUsingOnlineIdAuthenticator(new[]
                            {"wl.signin", "onedrive.readonly"});

                        await oneDriveClient.AuthenticateAsync();

                        using (
                            var responseStream =
                                await
                                    oneDriveClient.Drive.Root.ItemWithPath("KeysExport.xml")
                                        .Content.Request()
                                        .GetAsync())
                        {
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                await responseStream.CopyToAsync(memoryStream);
                                memoryStream.Position = 0;
                                if (!ImportData(memoryStream, true, true)) return;
                            }
                        }

                        DataItems = await _datatarget.GetAllData();
                        RaisePropertyChanged("DataItems");
                        TextToastManager.Instance.ShowToast(_loader.GetString("loadednimported"));
                    }
                    catch (Exception ex)
                    {
                        TextToastManager.Instance.ShowToast($"msdnus:Err:{ex.Message}");
                    }
                    finally
                    {
                        SetProgress(false);
                    }
                });

                //AboutPageCommand
                AboutPageCommand = new RelayCommand(() =>
                {
                    NavigationService.NavigateTo("About");
                });

                //CoffeeCupCommand
                CoffeeCupCommand = new RelayCommand(() =>
                {
                    NavigationService.NavigateTo("CoffeeCup");
                });

                //HelpCommand
                HelpCommand = new RelayCommand(() =>
                {
                    NavigationService.NavigateTo("Help");
                });

                //SettingsCommand
                SettingsCommand = new RelayCommand(() =>
                {
                    NavigationService.NavigateTo("Settings");
                });

                #endregion

                DataItems = await _datatarget.GetAllData();
                RaisePropertyChanged("DataItems");

                if (_settingsProvider.IsFirstRun)
                {
                    NavigationService.NavigateTo("Help");
                    _settingsProvider.IsFirstRun = false;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                TextToastManager.Instance.ShowToast($"msdnus:Err:Init:{ex.Message}");
#else
                TextToastManager.Instance.ShowToast($"msdnus:{ex.Message}");
#endif
            }
            finally
            {
                SetProgress(false);
            }
        }

        private async Task<bool> ImportData(StorageFile apath, bool iscleanupbefore, bool isdistinct)
        {
            YourKey zz = await _datasource.GetData(apath);
            return SaveData(zz, iscleanupbefore, isdistinct);
        }

        private bool ImportData(Stream stream, bool iscleanupbefore, bool isdistinct)
        {
            YourKey zz = _datasource.GetData(stream);
            return SaveData(zz, iscleanupbefore, isdistinct);
        }

        private bool SaveData(YourKey zz, bool iscleanupbefore, bool isdistinct)
        {
            List<YourKeyDTO> lst = new List<YourKeyDTO>();
            foreach (YourKeyProduct_Key ff in zz.Items)
            {
                YourKeyDTO dt = new YourKeyDTO();
                dt.MapFromYourKeyProduct_Key(ff);
                lst.Add(dt);
            }
            return _datatarget.SaveData(lst, iscleanupbefore, isdistinct);
        }

        private bool EnsureUnsnapped()
        {
            // FilePicker APIs will not work if the application is in a snapped state.
            // If an app wants to show a FilePicker while snapped, it must attempt to unsnap first
            bool unsnapped = ((ApplicationView.Value != ApplicationViewState.Snapped) || ApplicationView.TryUnsnap());
            if (!unsnapped)
            {
                TextToastManager.Instance.ShowToast(_loader.GetString("cantunsnap"));
            }

            return unsnapped;
        }

        private void SetProgress(bool isShow)
        {
            if (isShow)
            {
                StatusText = _loader.GetString("statusloading");
                IsProgress = Visibility.Visible;
            }
            else
            {
                StatusText = string.Empty;
                IsProgress = Visibility.Collapsed;
            }
        }
    }
}
