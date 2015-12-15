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
            _db = new DbInstance("msdnusdb");
            _db.Map<YourKeyDTO>()
                .Automap(i => i.Id, true)
                .WithIndex("Name", i => i.Name, StringComparer.OrdinalIgnoreCase);

            _db.Initialize();
        }


        public bool SaveData(List<YourKeyDTO> ykl)
        {
            try
            {
                _db.Save(ykl.ToArray());
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
            return tmp.ToList();
        }

        public async Task<List<YourKeyDTO>> GetDataValueByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return await GetAllData();

            var keys = _db.Table<YourKeyDTO>();
            var index = keys.IndexQuery<string>("Name").Where(x => (x ?? "").Contains("3")).Take(2);
            return await index.ToListAsync();
        }
    }
}
