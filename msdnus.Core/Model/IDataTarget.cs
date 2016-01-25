using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace msdnus.Core.Model
{
    public interface IDataTarget: IDisposable
    {
        bool SaveData(List<YourKeyDTO> ykl, bool iscleanbefore, bool isdistinct);
        Task<List<YourKeyDTO>> GetAllDataAsync();
        Task<List<YourKeyDTO>> GetDataValueByNameAsync(string name);
        Task<bool> CleanDbAsync();
    }
}
