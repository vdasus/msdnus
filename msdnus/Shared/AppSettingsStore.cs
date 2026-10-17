using System.IO;
using System.Text.Json;
using msdnus.Core.Model;

namespace msdnus.Shared
{
    /// <summary>
    /// Loads and persists <see cref="AppSettings"/> as plain JSON in the portable
    /// data folder (next to the executable when unpackaged). Holds no secrets.
    /// </summary>
    public sealed class AppSettingsStore
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        private readonly string _path;

        public AppSettings Settings { get; private set; }

        public AppSettingsStore()
        {
            _path = Path.Combine(AppEnvironment.DataFolder, "settings.json");
            Settings = Load();
        }

        private AppSettings Load()
        {
            try
            {
                if (File.Exists(_path))
                {
                    var loaded = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_path));
                    if (loaded != null) return loaded;
                }
            }
            catch (JsonException)
            {
                // corrupt settings file — start clean
            }

            return new AppSettings();
        }

        public void Save()
        {
            // Write-then-rename: settings.json is the commit point of a PIN change, a torn write would lose the vault.
            var tmp = _path + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(Settings, JsonOptions));
            File.Move(tmp, _path, overwrite: true);
        }
    }
}
