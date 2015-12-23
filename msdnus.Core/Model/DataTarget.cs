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

        public DataTarget()
        {
            Init();
        }

        private void Init()
        {
            //, StringComparer.OrdinalIgnoreCase
            _db = new DbInstance("msdnusdb");
            _db.Map<YourKeyDTO>()
                .Automap(i => i.Id, true)
                .WithIndex("Name", i => i.Name);
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

        public async Task<List<YourKeyDTO>> GetAllData()
        {
            var tmp = await _db.Table<YourKeyDTO>().LoadAllAsync();
            return tmp.OrderBy(x => x.Name).ToList();
        }

        public async Task<List<YourKeyDTO>> GetDataValueByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return await GetAllData();

            var keys = _db.Table<YourKeyDTO>();
            var index =
                keys.IndexQuery<string>("Name").Where(x => (x ?? string.Empty).ToUpper().Contains(name.ToUpper()));
            return await index.ToListAsync();
        }
    }
}
