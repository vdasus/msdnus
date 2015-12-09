using System.Threading.Tasks;
using msdnus.Core.Shared;

namespace msdnus.Core.Model
{
    public class FileDataSource: IDataSource
    {
        public async Task<YourKey> GetData(string source)
        {
            return await Utils.ReadObjectFromXmlFileAsync<YourKey>(source);
        }
    }
}
