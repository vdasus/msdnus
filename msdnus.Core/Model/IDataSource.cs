using System.Threading.Tasks;

namespace msdnus.Core.Model
{
    public interface IDataSource
    {
        Task<YourKey> GetData(string source);
    }
}
