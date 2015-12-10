using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.Storage;
using GalaSoft.MvvmLight;
using msdnus.Core.Model;
using msdnus.Shared;

namespace msdnus.ViewModel
{
    [Magic]
    public class MainViewModel: ViewModelBase
    {
        public string AppVersion { get; } =
    $"v.{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";

        public string TempRezult { get; set; }
        public string TempFolder { get; set; }

        public List<YourKeyDTO> TempList { get; set; }
        
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
                TempRezult = string.Empty;

                /*YourKey zz = await _datasource.GetData(@"KeysExport.xml");
                foreach (var xx in zz.Items)
                {
                    TempRezult += "\n" + xx.Name;
                    foreach (var ff in xx.Key)
                    {
                        var tt = "\n\t" + ff.ID + " " + ff.ClaimedDate + " " + ff.Type + " " + ff.Value;
                        TempRezult += tt.Replace("&gt;", ">").Replace("&lt;", "<");
                    }
                }

                List<YourKeyDTO> lst = new List<YourKeyDTO>();

                foreach (var ff in zz.Items)
                {
                    YourKeyDTO dt = new YourKeyDTO();
                    dt.MapFromYourKeyProduct_Key(ff);
                    lst.Add(dt);
                }

                _datatarget.SaveData(lst);*/

                TempList = _datatarget.GetAllData();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
