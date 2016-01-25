using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace msdnus.Core.Model
{
    //https://msdn.microsoft.com/en-us/subscriptions/securejson/GetAllExportKeys?brand=MSDN

    public interface IDataSource: IDisposable
    {
        Task<YourKey> GetDataAsync(StorageFile source);
        YourKey GetDataAsync(Stream stream);
        Task<bool> SaveDataAsync<T>(T objectToSave, string source);
    }
}
