using System.Collections.Generic;
using System.Threading.Tasks;

namespace msdnus.Core.Model
{
    public interface IDataTarget
    {
        bool SaveData(List<YourKeyDTO> ykl);
        List<YourKeyDTO> GetAllData();
    }
}
