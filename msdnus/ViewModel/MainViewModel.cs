using System;
using Windows.ApplicationModel;
using GalaSoft.MvvmLight;
using msdnus.Model;
using msdnus.Shared;

namespace msdnus.ViewModel
{
    [Magic]
    public class MainViewModel: ViewModelBase
    {
        public string AppVersion { get; } =
    $"v.{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";

        public string TempRezult { get; set; }
        

        public MainViewModel()
        {
            Init();
        }

        private async void Init()
        {
            try
            {
                TempRezult = string.Empty;

                YourKey zz = await Utils.ReadObjectFromXmlFileAsync<YourKey>(@"KeysExport.xml");
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
