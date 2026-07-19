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

        public void ChangePassword(string? newPassword)
        {
            // Re-encrypt by reading all documents, then rewriting them into a fresh
            // database file encrypted with the new password. Robust and cheap for the
            // small key stores used here, and independent of LiteDB's Rebuild quirks.
            var all = Keys.FindAll().ToList();

            _db.Dispose();
            File.Delete(_databasePath);

            _db = OpenDatabase(_databasePath, newPassword);
            Keys.EnsureIndex(x => x.Name);
            if (all.Count > 0) Keys.Insert(all);
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
