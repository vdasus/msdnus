using System;
using System.Collections.Generic;
using System.Linq;
using Lex.Db;

namespace msdnus.Core.Model
{
    public class DataTarget: IDataTarget
    {
        private readonly DbInstance _db;

        public DataTarget()
        {
            _db = new DbInstance("msdnus_db");
            _db.Map<YourKeyDTO>().Automap(i => i.Id, true);
            _db.Map<YourKeyKeyDTO>().Automap(i => i.Id, true);
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

        public List<YourKeyDTO> GetAllData()
        {
            return _db.LoadAll<YourKeyDTO>().ToList();
        }
    }
}
