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
            File.WriteAllText(_path, JsonSerializer.Serialize(Settings, JsonOptions));
        }
    }
}
