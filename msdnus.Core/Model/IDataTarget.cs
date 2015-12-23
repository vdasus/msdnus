using System.Collections.Generic;
using System.Threading.Tasks;

namespace msdnus.Core.Model
{
    public interface IDataTarget
    {
        bool SaveData(List<YourKeyDTO> ykl, bool iscleanbefore, bool isdistinct);
        Task<List<YourKeyDTO>> GetAllData();
        Task<List<YourKeyDTO>> GetDataValueByName(string name);
    }
}
