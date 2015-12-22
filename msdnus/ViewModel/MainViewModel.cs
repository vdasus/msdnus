#define zLOADDATA

using System;
using System.Collections.Generic;
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

namespace msdnus.ViewModel
{
    [Magic]
    public class MainViewModel: PropertyChangedBase
    {
        public string AppVersion { get; } =
    $"v.{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";

        private readonly ResourceLoader _loader;

        private readonly INavigationService _navigationService;

        public string TempRezult { get; set; }
        
        public string TextToSearch { get; set; }

        public List<YourKeyDTO> TempList { get; set; }

        #region commands
        public RelayCommand<string> CopyToClipboard { get; set; }
        public RelayCommand SearchStringChanged { get; set; }
        public RelayCommand GetFromOneDrive { get; set; }
        public RelayCommand LockAll { get; set; }
        #endregion

        private readonly IDataSource _datasource;
        private readonly IDataTarget _datatarget;

        public MainViewModel(IDataSource datasource, IDataTarget datatarget, INavigationService navigationService)
        {
            _navigationService = navigationService;
            
            _datasource = datasource;
            _datatarget = datatarget;
            
            _loader = new ResourceLoader();
            
            Init();
        }

        private async void Init()
        {
            try
            {
                #region commands
                //CopyToClipboard
                CopyToClipboard = new RelayCommand<string>((e) =>
                {
                    /*MessageDialog msg = new MessageDialog(e);
                    await msg.ShowAsync();*/
                    DataPackage dp = new DataPackage();
                    dp.SetText(e);
                    Clipboard.SetContent(dp);
                    TextToastManager.Instance.ShowToast(_loader.GetString("copyedtext"));
                });

                //SearchStringChanged
                SearchStringChanged = new RelayCommand(async () =>
                {
                    TempList = await _datatarget.GetDataValueByName(TextToSearch ?? string.Empty);
                    RaisePropertyChanged("TempList");
                });

                //GetFromOneDrive
                GetFromOneDrive = new RelayCommand(async () =>
                {

                    //ClID: 000000004017CE14
                    //ClSc: 3Iq0D38qYplvEhpoLGpVpidVXpiEdf1X
                    /*FileOpenPicker fp = new FileOpenPicker();
                    await fp.PickSingleFileAsync();*/

                    try
                    {
                        /*var oneDriveClient = OneDriveClientExtensions.GetClientUsingOnlineIdAuthenticator(new[] { "wl.signin", "onedrive.readonly" });

                        /*var oneDriveClient = OneDriveClient.GetMicrosoftAccountClient("000000004017CE14",
                            "http://vdasus.com", new[] {"wl.signin", "onedrive.readonly"},
                            "3Iq0D38qYplvEhpoLGpVpidVXpiEdf1X");#1#

                        await oneDriveClient.AuthenticateAsync();
                        var drive = await oneDriveClient
                            .Drive
                            .Request()
                            .GetAsync();
                        var zzz = drive.Items;*/

                        if (EnsureUnsnapped())
                        {
                            FileOpenPicker openPicker = new FileOpenPicker
                            {
                                ViewMode = PickerViewMode.Thumbnail,
                                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                            };
                            openPicker.FileTypeFilter.Add(".xml");
                            
                            StorageFile file = await openPicker.PickSingleFileAsync();

                            if (file == null) return;
                            if (!await ImportData(file, true)) return;

                            TempList = await _datatarget.GetAllData();
                            RaisePropertyChanged("TempList");
                            TextToastManager.Instance.ShowToast(_loader.GetString("loadednimported"));
                        }
                    }
                    catch (Exception ex)
                    {
                        TextToastManager.Instance.ShowToast($"msdnus:Err:{ex.Message}");
                    }
                });

                //LockAll
                LockAll = new RelayCommand(() =>
                {
                    _navigationService.NavigateTo("Iejimo");
                });
                #endregion

                TempList = await _datatarget.GetAllData();
                RaisePropertyChanged("TempList");
            }
            catch (Exception ex)
            {
                TextToastManager.Instance.ShowToast($"msdnus:Err:{ex.Message}");
            }
        }

        private async Task<bool> ImportData(StorageFile apath, bool iscleanupbefore)
        {
            YourKey zz = await _datasource.GetData(apath);
            List<YourKeyDTO> lst = new List<YourKeyDTO>();
            foreach (var ff in zz.Items)
            {
                YourKeyDTO dt = new YourKeyDTO();
                dt.MapFromYourKeyProduct_Key(ff);
                lst.Add(dt);
            }
            return _datatarget.SaveData(lst, iscleanupbefore);
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
