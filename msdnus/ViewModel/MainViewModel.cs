using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Popups;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using msdnus.Core.Model;
using msdnus.Shared;

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

                CopyToClipboard = new RelayCommand<string>(async (e) =>
                {
                    MessageDialog msg = new MessageDialog(e);
                    await msg.ShowAsync();
                    DataPackage dp = new DataPackage();
                    dp.SetText(e);
                    Clipboard.SetContent(dp);
                });

                SearchStringChanged = new RelayCommand(async () =>
                {
                    TempList = await _datatarget.GetDataValueByName(TextToSearch ?? string.Empty);
                    RaisePropertyChanged("TempList");
                });

                TempList = await _datatarget.GetAllData();
                RaisePropertyChanged("TempList");
            }
            catch (Exception ex)
            {

            }
        }
    }
}
