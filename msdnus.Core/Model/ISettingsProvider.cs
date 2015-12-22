using System.Threading.Tasks;

namespace msdnus.Core.Model
{
    public interface ISettingsProvider
    {
        bool FirstRun { get; set; }
        Task<string> GetIejimoParameter();
        Task<bool> SetIejimoParameter(string astr);
    }
}