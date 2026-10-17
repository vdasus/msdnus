using LiteDB;

namespace msdnus.Core.Model
{
    /// <summary>
    /// LiteDB-backed key store. The database file is encrypted with a password
    /// (derived from the user PIN by the caller), so key values are protected at
    /// rest without per-field encryption.
    /// </summary>
    public class DataTarget : IDataTarget
    {
        private const string CollectionName = "keys";

        private readonly string _databasePath;
        private LiteDatabase _db;
        private bool _disposed;

        public DataTarget(string databasePath, string? password = null)
        {
            _databasePath = databasePath;
            _db = OpenDatabase(databasePath, password);
            Keys.EnsureIndex(x => x.Name);
        }

        private static LiteDatabase OpenDatabase(string path, string? password) =>
            new(new ConnectionString
            {
                Filename = path,
                Password = string.IsNullOrEmpty(password) ? null : password
            });

        private ILiteCollection<YourKeyDTO> Keys => _db.GetCollection<YourKeyDTO>(CollectionName);

        public bool SaveData(List<YourKeyDTO> ykl, bool iscleanbefore, bool isdistinct)
        {
            try
            {
                if (iscleanbefore)
                {
                    Keys.DeleteAll();
                }

                var rez = isdistinct ? ykl.Distinct(new YourKeyDTOComparer()).ToList() : ykl;
                Keys.Insert(rez);
                return true;
            }
            catch (LiteException)
            {
                return false;
            }
        }

        public Task<List<YourKeyDTO>> GetAllDataAsync()
        {
            var all = Keys.FindAll().OrderBy(x => x.Name).ToList();
            return Task.FromResult(all);
        }

        public Task<List<YourKeyDTO>> GetDataValueByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name)) return GetAllDataAsync();

            var found = Keys.FindAll()
                .Where(x => (x.Name ?? string.Empty).Contains(name, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.Name)
                .ToList();
            return Task.FromResult(found);
        }

        public Task<bool> CleanDbAsync()
        {
            try
            {
                Keys.DeleteAll();
                return Task.FromResult(true);
            }
            catch (LiteException)
            {
                return Task.FromResult(false);
            }
        }

        public void ExportTo(string targetPath, string? password)
        {
            // Writes a copy into a fresh file instead of rewriting in place: the source stays
            // intact until the caller commits, and a stale "-log" file next to the old database
            // (e.g. one a sync client kept locked) can never be mixed with the new password.
            var all = Keys.FindAll().ToList();

            using var target = OpenDatabase(targetPath, password);
            var keys = target.GetCollection<YourKeyDTO>(CollectionName);
            keys.EnsureIndex(x => x.Name);
            if (all.Count > 0) keys.Insert(all);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _db.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
