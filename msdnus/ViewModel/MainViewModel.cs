using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using GalaSoft.MvvmLight.Command;
using msdnus.Core.Model;
using msdnus.Shared;
using Microsoft.OneDrive.Sdk;

namespace msdnus.ViewModel
{
    [Magic]
    public class MainViewModel: PropertyChangedBase
    {
        public string AppVersion { get; } =
    $"v.{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";

        public string TempRezult { get; set; }
        public string TempFolder { get; set; }

        public string TextToSearch { get; set; }

        public List<YourKeyDTO> TempList { get; set; }

        public RelayCommand<string> CopyToClipboard { get; set; }
        public RelayCommand SearchStringChanged { get; set; }
        public RelayCommand GetFromOneDrive { get; set; }

        private readonly IDataSource _datasource;
        private readonly IDataTarget _datatarget;

        public MainViewModel(IDataSource datasource, IDataTarget datatarget)
        {
            TempFolder = ApplicationData.Current.LocalFolder.Path;
            _datasource = datasource;
            _datatarget = datatarget;

            Init();
        }

        private async void Init()
        {
            try
            {
                /*YourKey zz = await _datasource.GetData(@"KeysExport.xml");
                
                List<YourKeyDTO> lst = new List<YourKeyDTO>();

                foreach (var ff in zz.Items)
                {
                    YourKeyDTO dt = new YourKeyDTO();
                    dt.MapFromYourKeyProduct_Key(ff);
                    lst.Add(dt);
                }

                _datatarget.SaveData(lst);*/

                TempList = await _datatarget.GetAllData();
                RaisePropertyChanged("TempList");

                #region commands
                CopyToClipboard = new RelayCommand<string>((e) =>
                {
                    /*MessageDialog msg = new MessageDialog(e);
                    await msg.ShowAsync();*/
                    DataPackage dp = new DataPackage();
                    dp.SetText(e);
                    Clipboard.SetContent(dp);
                    TextToastManager.Instance.ShowToast("Copied.");
                });

                SearchStringChanged = new RelayCommand(async () =>
                {
                    TempList = await _datatarget.GetDataValueByName(TextToSearch ?? string.Empty);
                    RaisePropertyChanged("TempList");
                });

                GetFromOneDrive = new RelayCommand(async () =>
                {

                    //ClID: 000000004017CE14
                    //ClSc: 3Iq0D38qYplvEhpoLGpVpidVXpiEdf1X
                    /*FileOpenPicker fp = new FileOpenPicker();
                    await fp.PickSingleFileAsync();*/

                    var oneDriveClient = OneDriveClient.GetMicrosoftAccountClient("000000004017CE14",
                        "http://vdasus.com", new[] {"wl.signin", "onedrive.readonly"},
                        "3Iq0D38qYplvEhpoLGpVpidVXpiEdf1X");
                    await oneDriveClient.AuthenticateAsync();
                    var drive = await oneDriveClient
                          .Drive
                          .Request()
                          .GetAsync();
                    var zz = drive.Items;
                });
                #endregion
            }
            catch (Exception ex)
            {

            }
        }
    }
}
