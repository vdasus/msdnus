using System.Threading.Tasks;

namespace msdnus.Core.Model
{
    public interface ISettingsProvider
    {
        bool IsFirstRun { get; set; }
        bool IsDarkTheme { get; set; }
        Task<string> GetIejimoParameter();
        Task<bool> SetIejimoParameter(string astr);
    }
}