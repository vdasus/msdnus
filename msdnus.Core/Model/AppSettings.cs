using System.Collections.Generic;

namespace msdnus.Core.Model
{
    /// <summary>One MSDN account/profile: a display name plus its own encrypted database file.</summary>
    public sealed class ProfileInfo
    {
        public string Name { get; set; } = string.Empty;

        /// <summary>Database file name (stable, GUID-based) — decoupled from the display name so profiles can be renamed.</summary>
        public string File { get; set; } = string.Empty;
    }

    /// <summary>
    /// Portable application settings, stored as plain JSON next to the executable.
    /// Contains no secrets: the salt and PIN verifier do not reveal the PIN, and the
    /// key material never leaves memory.
    /// </summary>
    public sealed class AppSettings
    {
        public int Schema { get; set; } = 2;

        /// <summary>Random per-vault PBKDF2 salt (base64). Not secret.</summary>
        public string? KdfSalt { get; set; }

        public int KdfIterations { get; set; } = 300_000;

        /// <summary>PBKDF2-derived verifier used to check the PIN without storing it.</summary>
        public string? PinVerifier { get; set; }

        public List<ProfileInfo> Profiles { get; set; } = new();

        public string? SelectedProfile { get; set; }

        public bool IsDarkTheme { get; set; }
    }
}
