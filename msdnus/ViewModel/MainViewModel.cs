using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using msdnus.Core.Model;
using msdnus.Core.Shared;
using msdnus.Shared;
using Microsoft.OneDrive.Sdk;

// ReSharper disable ExplicitCallerInfoArgument
namespace msdnus.ViewModel
{
    [Magic]
    public sealed class MainViewModel: PropertyChangedBase
    {
        private const int MAX_ERR_COUNT = 24;

        private readonly ResourceLoader _loader;

        private readonly INavigationService _navigationService;
        private readonly ISettingsProvider _settingsProvider;

        public NotifyTaskCompletion<List<YourKeyDTO>> DataItems { get; set; }

        public int IejiTryCnt { get; set; }
        
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
        public RelayCommand ResetIejimoPwdCommand { get; set; }
        #endregion

        private readonly IDataSource _datasource;
        private readonly IDataTarget _datatarget;

        //public ITaskCompletionNotifier Initialization { get; private set; }

        public MainViewModel(IDataSource datasource, IDataTarget datatarget, INavigationService navigationService, ISettingsProvider settingsProvider)
        {
            _datasource = datasource;
            _datatarget = datatarget;
            _navigationService = navigationService;
            _settingsProvider = settingsProvider;

            _loader = ResourceLoader.GetForCurrentView();

            Init();
        }

        private void Init()
        {
            SetProgress(true);
            
            try
            {
                #region commands

                //CheckPassword
                CheckPassword = new RelayCommand<string>(async e =>
                {
                    if (IejiTryCnt++ < MAX_ERR_COUNT)
                    {

                        //await _settingsProvider.SetIejimoParameter("123");
                        var tiejimas = await _settingsProvider.GetIejimoParameter();

                        if (string.IsNullOrEmpty(tiejimas) || e != tiejimas) return;

                        IsIejimoMode = false;
                        RaisePropertyChanged("IsIejimoMode");
                        IejiTryCnt = 0;
                    }
                    else if (ResetIejimoPwdCommand.CanExecute(null))
                    {
                        ResetIejimoPwdCommand.Execute(null);
                    }
                });

                //CopyToClipboard
                CopyToClipboardCommand = new RelayCommand<string>(e =>
                {
                    DataPackage dp = new DataPackage();
                    dp.SetText(e);
                    Clipboard.SetContent(dp);
                    TextToastManager.Instance.ShowToast(_loader.GetString("copyedtext"));
                });

                //SearchStringChanged
                SearchStringChangedCommand =
                    new RelayCommand(
                        () =>
                        {
                            DataItems =
                                new NotifyTaskCompletion<List<YourKeyDTO>>(
                                    _datatarget.GetDataValueByName(TextToSearch ?? string.Empty));
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

                        if (!await ImportDataAsync(file, true, true)) return;

                        DataItems = new NotifyTaskCompletion<List<YourKeyDTO>>(_datatarget.GetAllData());
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
                LockAllCommand = new RelayCommand(() => { IsIejimoMode = true; });

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
                                if (!ImportDataAsync(memoryStream, true, true)) return;
                            }
                        }

                        DataItems = new NotifyTaskCompletion<List<YourKeyDTO>>(_datatarget.GetAllData());
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
                AboutPageCommand = new RelayCommand(() => { _navigationService.NavigateTo("About"); });

                //CoffeeCupCommand
                CoffeeCupCommand = new RelayCommand(() => { _navigationService.NavigateTo("CoffeeCup"); });

                //HelpCommand
                HelpCommand = new RelayCommand(() => { _navigationService.NavigateTo("Help"); });

                //SettingsCommand
                SettingsCommand = new RelayCommand(() => { _navigationService.NavigateTo("Settings"); });

                //ResetIejimoPwdCommand
                ResetIejimoPwdCommand = new RelayCommand(async () =>
                {
                    MessageDialog msg = new MessageDialog(_loader.GetString("rusure"));
                    msg.Commands.Add(new UICommand {Label = "Ok", Id = 0});
                    msg.Commands.Add(new UICommand {Label = "Cancel", Id = 1});
                    var rez = await msg.ShowAsync();

                    if ((int) rez.Id != 0) return;

                    IejiTryCnt = 0;
                    await _datatarget.CleanDb();
                    await _settingsProvider.SetIejimoParameter("123");
                    DataItems = new NotifyTaskCompletion<List<YourKeyDTO>>(_datatarget.GetAllData());
                    TextToastManager.Instance.ShowToast(_loader.GetString("iejreset"));
                });

                #endregion

                if (_settingsProvider.IsFirstRun)
                {
                    /*_navigationService.NavigateTo("Help");*/
                    /*IsIejimoMode = true;*/
                    if (ResetIejimoPwdCommand.CanExecute(null)) ResetIejimoPwdCommand.Execute(null);
                    _settingsProvider.IsFirstRun = false;
                }

                try
                {
                    ThemeAwareFrame rootFrame = Window.Current.Content as ThemeAwareFrame;
                    if (rootFrame != null)
                        rootFrame.AppTheme = _settingsProvider.IsDarkTheme ? ElementTheme.Dark : ElementTheme.Light;
                }
                catch
                {
                    //ignored
                }

                if (LockAllCommand.CanExecute(null)) LockAllCommand.Execute(null);

                DataItems = new NotifyTaskCompletion<List<YourKeyDTO>>(_datatarget.GetAllData());
            }
            catch (Exception ex)
            {
#if DEBUG
                TextToastManager.Instance.ShowToast($"msdnus:ERR:Init:{ex.StackTrace}");
#else
                TextToastManager.Instance.ShowToast($"msdnus:{ex.Message}");
#endif
            }
            finally
            {
                SetProgress(false);
            }
        }

        private async Task<bool> ImportDataAsync(StorageFile apath, bool iscleanupbefore, bool isdistinct)
        {
            YourKey zz = await _datasource.GetDataAsync(apath);
            return SaveData(zz, iscleanupbefore, isdistinct);
        }

        private bool ImportDataAsync(Stream stream, bool iscleanupbefore, bool isdistinct)
        {
            YourKey zz = _datasource.GetDataAsync(stream);
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
            bool unsnapped = (ApplicationView.Value != ApplicationViewState.Snapped) || ApplicationView.TryUnsnap();
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
