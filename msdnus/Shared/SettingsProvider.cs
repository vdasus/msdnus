using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using msdnus.Core.Model;
using msdnus.Core.Shared;

namespace msdnus.Shared
{
    /// <summary>
    /// Settings store. Uses ApplicationData.LocalSettings when running packaged,
    /// and a JSON file under the local app-data folder when running unpackaged.
    /// </summary>
    public class SettingsProvider : ISettingsProvider
    {
        private readonly ICryptoService _crypto;
        private readonly Dictionary<string, object?> _fileValues = new();
        private readonly string? _settingsFilePath;
        private readonly Windows.Storage.ApplicationDataContainer? _localSettings;

        public SettingsProvider(ICryptoService crypto)
        {
            _crypto = crypto;

            if (AppEnvironment.IsPackaged)
            {
                _localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            }
            else
            {
                _settingsFilePath = Path.Combine(AppEnvironment.DataFolder, "settings.json");
                if (File.Exists(_settingsFilePath))
                {
                    try
                    {
                        var doc = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                            File.ReadAllText(_settingsFilePath));
                        if (doc != null)
                        {
                            foreach (var (key, el) in doc)
                            {
                                _fileValues[key] = el.ValueKind switch
                                {
                                    JsonValueKind.True => true,
                                    JsonValueKind.False => false,
                                    JsonValueKind.String => el.GetString(),
                                    _ => null
                                };
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        // corrupt settings file — start clean
                    }
                }
            }
        }

        private object? Get(string key) =>
            _localSettings != null ? _localSettings.Values[key] : _fileValues.GetValueOrDefault(key);

        private void Set(string key, object? value)
        {
            if (_localSettings != null)
            {
                _localSettings.Values[key] = value;
                return;
            }

            _fileValues[key] = value;
            File.WriteAllText(_settingsFilePath!, JsonSerializer.Serialize(_fileValues));
        }

        private bool GetBool(string key, bool defaultValue)
        {
            var value = Get(key);
            if (value is bool b) return b;
            Set(key, defaultValue);
            return defaultValue;
        }

        public bool IsFirstRun
        {
            get => GetBool("IsFirstRun", true);
            set => Set("IsFirstRun", value);
        }

        public bool IsDarkTheme
        {
            get => GetBool("IsDarkTheme", false);
            set => Set("IsDarkTheme", value);
        }

        public bool IsCoffeCuped
        {
            get => GetBool("IsCoffeCuped", false);
            set => Set("IsCoffeCuped", value);
        }

        public string? GetIejimoParameterAsync()
        {
            try
            {
                var value = Get("IejiPar");

                if (value == null)
                {
                    SetIejimoParameter("123");
                    value = Get("IejiPar");
                }

                return _crypto.Unprotect((string?)value);
            }
            catch
            {
                return null;
            }
        }

        public bool SetIejimoParameter(string astr)
        {
            try
            {
                var tmp = _crypto.Protect(astr);
                if (tmp == null) return false;
                Set("IejiPar", tmp);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
