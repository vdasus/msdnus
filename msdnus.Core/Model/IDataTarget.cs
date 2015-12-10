using System.Collections.Generic;
using System.Threading.Tasks;

namespace msdnus.Core.Model
{
    public interface IDataTarget
    {
        Task<bool> SaveData(List<YourKeyDTO> yk);
    }
}
