using System.Threading.Tasks;

namespace msdnus.Core.Model
{
    public interface ISettingsProvider
    {
        bool IsFirstRun { get; set; }
        bool IsDarkTheme { get; set; }
        bool IsCoffeCuped { get; set; }

        /// <summary>
        /// Get iejimo parameter, null if unsuccessful
        /// </summary>
        /// <returns>null if unsuccessful</returns>
        Task<string> GetIejimoParameterAsync();

        /// <summary>
        /// Set Iejimo parameter, true if successful
        /// </summary>
        /// <param name="astr">string to set</param>
        /// <returns>true if successful</returns>
        Task<bool> SetIejimoParameterAsync(string astr);
    }
}