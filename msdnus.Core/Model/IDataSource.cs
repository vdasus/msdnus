using System.Threading.Tasks;

namespace msdnus.Core.Model
{
    //https://msdn.microsoft.com/en-us/subscriptions/securejson/GetAllExportKeys?brand=MSDN

    public interface IDataSource
    {
        Task<YourKey> GetData(string source);
        Task<bool> SaveData<T>(T objectToSave, string source);
    }
}
