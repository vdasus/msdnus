using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lex.Db;

namespace msdnus.Core.Model
{
    public class DataTarget: IDataTarget
    {
        private DbInstance _db;
        private bool _disposed;

        public DataTarget()
        {
            Init();
        }

        private void Init()
        {
            _db = new DbInstance("msdnusdb");
            _db.Map<YourKeyDTO>()
                .Automap(i => i.Id, true)
                .WithIndex("Name", i => i.Name, StringComparer.OrdinalIgnoreCase);
            _db.Initialize();
        }

        public bool SaveData(List<YourKeyDTO> ykl, bool iscleanbefore, bool isdistinct)
        {
            try
            {
                if (iscleanbefore)
                {
                    _db.Purge();
                    _db.Compact();
                }
                var rez = isdistinct ? ykl.Distinct(new YourKeyDTOComparer()).ToArray() : ykl.ToArray();
                _db.Save(rez);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public async Task<List<YourKeyDTO>> GetAllDataAsync()
        {
            var tmp = await _db.Table<YourKeyDTO>().LoadAllAsync();
            return tmp.OrderBy(x => x.Name).ToList();
        }

        public async Task<List<YourKeyDTO>> GetDataValueByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name)) return await GetAllDataAsync();

            var keys = _db.Table<YourKeyDTO>();
            var index =
                keys.IndexQuery<string>("Name").Where(x => (x ?? string.Empty).ToUpper().Contains(name.ToUpper()));
            return await index.ToListAsync();
        }

        public async Task<bool> CleanDbAsync()
        {
            try
            {
                await _db.PurgeAsync();
                await _db.CompactAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _db.Dispose();
            }
                
            _disposed = true;
        }

        ~DataTarget()
        {
            Dispose(false);
        }
    }
}
