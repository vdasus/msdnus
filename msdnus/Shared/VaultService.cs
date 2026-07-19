using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using msdnus.Core.Model;
using msdnus.Core.Shared;

namespace msdnus.Shared
{
    /// <summary>
    /// Owns the encrypted, portable vault: PIN-derived encryption, the set of MSDN
    /// profiles (one encrypted database each) and the currently opened profile.
    ///
    /// Security model: the AES key is derived from the user PIN plus a random per-vault
    /// salt via PBKDF2 (no machine binding), so the whole data folder can be synced across
    /// machines and unlocked with the PIN. Changing the PIN re-encrypts every profile.
    /// </summary>
    public sealed class VaultService : IDisposable
    {
        private const int SaltLength = 16;
        private const string VerifierPurpose = "pin-verify";

        private readonly AppSettingsStore _store;

        private AesGcmCryptoService? _crypto;
        private IDataTarget? _current;

        public VaultService(AppSettingsStore store) => _store = store;

        public AppSettings Settings => _store.Settings;

        /// <summary>True once a PIN has been created (i.e. the vault is initialised).</summary>
        public bool HasPin => !string.IsNullOrEmpty(Settings.PinVerifier) && !string.IsNullOrEmpty(Settings.KdfSalt);

        public bool IsUnlocked => _crypto != null;

        public IDataTarget? Current => _current;

        public IReadOnlyList<string> ProfileNames => Settings.Profiles.Select(p => p.Name).ToList();

        public string? SelectedProfile => Settings.SelectedProfile;

        // ---- PIN lifecycle -------------------------------------------------

        /// <summary>Creates the vault's first PIN and unlocks it. Used on first run / after a reset.</summary>
        public void CreatePin(string pin)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltLength);
            var crypto = new AesGcmCryptoService(pin, salt, AesGcmCryptoService.DefaultIterations);

            Settings.KdfSalt = Convert.ToBase64String(salt);
            Settings.KdfIterations = AesGcmCryptoService.DefaultIterations;
            Settings.PinVerifier = crypto.DeriveSecret(VerifierPurpose);
            _store.Save();

            _crypto = crypto;
            EnsureSelectedProfile();
            OpenSelectedProfile();
        }

        /// <summary>Validates the PIN and, on success, unlocks the vault.</summary>
        public bool TryUnlock(string pin)
        {
            if (!HasPin) return false;

            var salt = Convert.FromBase64String(Settings.KdfSalt!);
            var crypto = new AesGcmCryptoService(pin, salt, Settings.KdfIterations);
            if (!CryptographicOperations.FixedTimeEquals(
                    Convert.FromBase64String(crypto.DeriveSecret(VerifierPurpose)),
                    Convert.FromBase64String(Settings.PinVerifier!)))
            {
                return false;
            }

            _crypto = crypto;
            EnsureSelectedProfile();
            OpenSelectedProfile();
            return true;
        }

        /// <summary>Re-encrypts every profile with a new PIN. Must be unlocked.</summary>
        public bool ChangePin(string newPin)
        {
            if (_crypto == null) return false;

            var oldCrypto = _crypto;
            var newSalt = RandomNumberGenerator.GetBytes(SaltLength);
            var newCrypto = new AesGcmCryptoService(newPin, newSalt, AesGcmCryptoService.DefaultIterations);

            _current?.Dispose();
            _current = null;

            foreach (var p in Settings.Profiles)
            {
                var path = DbPath(p);
                if (!File.Exists(path)) continue;

                using var t = new DataTarget(path, oldCrypto.DeriveSecret(DbPurpose(p)));
                t.ChangePassword(newCrypto.DeriveSecret(DbPurpose(p)));
            }

            Settings.KdfSalt = Convert.ToBase64String(newSalt);
            Settings.KdfIterations = AesGcmCryptoService.DefaultIterations;
            Settings.PinVerifier = newCrypto.DeriveSecret(VerifierPurpose);
            _store.Save();

            _crypto = newCrypto;
            OpenSelectedProfile();
            return true;
        }

        public void Lock()
        {
            _current?.Dispose();
            _current = null;
            _crypto = null;
        }

        /// <summary>Erases all profile databases and PIN material. The vault returns to first-run state.</summary>
        public void ResetAll()
        {
            _current?.Dispose();
            _current = null;
            _crypto = null;

            foreach (var p in Settings.Profiles)
                TryDeleteDb(p);

            Settings.Profiles.Clear();
            Settings.SelectedProfile = null;
            Settings.KdfSalt = null;
            Settings.PinVerifier = null;
            _store.Save();
        }

        // ---- Profiles ------------------------------------------------------

        public void SelectProfile(string name)
        {
            if (_crypto == null || name == Settings.SelectedProfile) return;
            if (Settings.Profiles.All(p => p.Name != name)) return;

            Settings.SelectedProfile = name;
            _store.Save();
            OpenSelectedProfile();
        }

        public bool AddProfile(string name)
        {
            if (_crypto == null) return false;

            name = (name ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(name)) return false;
            if (Settings.Profiles.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase))) return false;

            var profile = new ProfileInfo { Name = name, File = NewDbFileName() };
            Settings.Profiles.Add(profile);
            Settings.SelectedProfile = name;
            _store.Save();
            OpenSelectedProfile();
            return true;
        }

        public bool RemoveProfile(string name)
        {
            if (_crypto == null) return false;
            if (Settings.Profiles.Count <= 1) return false;

            var profile = Settings.Profiles.FirstOrDefault(p => p.Name == name);
            if (profile == null) return false;

            var wasSelected = Settings.SelectedProfile == name;
            if (wasSelected)
            {
                _current?.Dispose();
                _current = null;
            }

            Settings.Profiles.Remove(profile);
            if (wasSelected) Settings.SelectedProfile = Settings.Profiles[0].Name;
            _store.Save();

            TryDeleteDb(profile);
            if (wasSelected) OpenSelectedProfile();
            return true;
        }

        // ---- Misc ----------------------------------------------------------

        public void SaveTheme(bool dark)
        {
            Settings.IsDarkTheme = dark;
            _store.Save();
        }

        public void Dispose() => _current?.Dispose();

        // ---- Internals -----------------------------------------------------

        private void EnsureSelectedProfile()
        {
            if (Settings.Profiles.Count == 0)
            {
                Settings.Profiles.Add(new ProfileInfo { Name = "Default", File = NewDbFileName() });
                Settings.SelectedProfile = "Default";
                _store.Save();
                return;
            }

            if (string.IsNullOrEmpty(Settings.SelectedProfile) ||
                Settings.Profiles.All(p => p.Name != Settings.SelectedProfile))
            {
                Settings.SelectedProfile = Settings.Profiles[0].Name;
                _store.Save();
            }
        }

        private void OpenSelectedProfile()
        {
            _current?.Dispose();
            _current = null;

            var profile = Settings.Profiles.First(p => p.Name == Settings.SelectedProfile);
            _current = new DataTarget(DbPath(profile), _crypto!.DeriveSecret(DbPurpose(profile)));
        }

        private static string NewDbFileName() => $"profile_{Guid.NewGuid():N}.litedb";

        private static string DbPurpose(ProfileInfo p) => "db:" + p.File;

        private static string DbPath(ProfileInfo p) => Path.Combine(AppEnvironment.DataFolder, p.File);

        private static void TryDeleteDb(ProfileInfo p)
        {
            try
            {
                var path = DbPath(p);
                if (File.Exists(path)) File.Delete(path);
            }
            catch
            {
                // best-effort cleanup
            }
        }
    }
}
