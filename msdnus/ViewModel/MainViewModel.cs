using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;
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
        public string AppVersion { get; } =
    $"v.{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";

        private readonly ResourceLoader _loader;

        private readonly INavigationService _navigationService;

        public List<YourKeyDTO> DataItems { get; set; }

        public string TextToSearch { get; set; }
        public string StatusText { get; set; }
        
        #region commands
        public RelayCommand<string> CopyToClipboardCommand { get; set; }
        public RelayCommand SearchStringChangedCommand { get; set; }
        public RelayCommand GetFromFilePickerCommand { get; set; }
        public RelayCommand LockAllCommand { get; set; }
        public RelayCommand OnedriveCommand { get; set; }
        #endregion

        private readonly IDataSource _datasource;
        private readonly IDataTarget _datatarget;

        //public ITaskCompletionNotifier Initialization { get; private set; }

        public MainViewModel(IDataSource datasource, IDataTarget datatarget, INavigationService navigationService)
        {
            _navigationService = navigationService;
            
            _datasource = datasource;
            _datatarget = datatarget;
            
            _loader = new ResourceLoader();

            //var Initialization = TaskCompletionNotifierFactory.Create(InitializeAsync());
            Init();
        }

        private async void Init()
        {
            StatusText = _loader.GetString("statusloading");
            
            try
            {
                #region commands

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

                        StatusText = _loader.GetString("statusloading");

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
                        StatusText = string.Empty;
                    }
                });

                //LockAll
                LockAllCommand = new RelayCommand(() =>
                {
                    _navigationService.NavigateTo("Iejimo");
                });

                //OnedriveCommand
                OnedriveCommand = new RelayCommand(async () =>
                {
                    //ClID: 000000004017CE14
                    //ClSc: 3Iq0D38qYplvEhpoLGpVpidVXpiEdf1X
                    try
                    {
                        StatusText = _loader.GetString("statusloading");
                        
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
                        StatusText = string.Empty;
                    }
                });

                #endregion

                DataItems = await _datatarget.GetAllData();
                RaisePropertyChanged("DataItems");
            }
            catch (Exception ex)
            {
                TextToastManager.Instance.ShowToast($"msdnus:Err:{ex.Message}");
            }
            finally
            {
                StatusText = string.Empty;
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

        internal bool EnsureUnsnapped()
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
    }
}
