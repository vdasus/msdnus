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

        /// <summary>Copies all data into a new database file encrypted with <paramref name="password"/> (unencrypted when null/empty). The source is left untouched.</summary>
        void ExportTo(string targetPath, string? password);
    }
}
