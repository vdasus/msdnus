using System;
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
        
        private readonly IDataSource _datasource;

        public MainViewModel(IDataSource datasource)
        {
            TempFolder = ApplicationData.Current.LocalFolder.Path;
            _datasource = datasource;
            
                Init();
        }

        private async void Init()
        {
            try
            {
                Lex.Db.Serialization.Extender.RegisterType<YourKeyKeyDTO, CustomTypesSerializers>(1000);

                TempRezult = string.Empty;

                YourKey zz = await _datasource.GetData(@"KeysExport.xml");
                foreach (var xx in zz.Items)
                {
                    TempRezult += "\n" + xx.Name;
                    foreach (var ff in xx.Key)
                    {
                        var tt = "\n\t" + ff.ID + " " + ff.ClaimedDate + " " + ff.Type + " " + ff.Value;
                        TempRezult += tt.Replace("&gt;", ">").Replace("&lt;", "<");
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
