using LiteDB;

namespace msdnus.Core.Model
{
    /// <summary>
    /// LiteDB-backed key store. The database file is encrypted with a password
    /// (derived from the machine-bound key by the caller), so key values are
    /// protected at rest without per-field encryption.
    /// </summary>
    public class DataTarget : IDataTarget
    {
        private const string CollectionName = "keys";

        private readonly LiteDatabase _db;
        private bool _disposed;

        public DataTarget(string databasePath, string? password = null)
        {
            var connectionString = new ConnectionString
            {
                Filename = databasePath,
                Password = string.IsNullOrEmpty(password) ? null : password
            };
            _db = new LiteDatabase(connectionString);
            Keys.EnsureIndex(x => x.Name);
        }

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

        public void Dispose()
        {
            if (_disposed) return;
            _db.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
