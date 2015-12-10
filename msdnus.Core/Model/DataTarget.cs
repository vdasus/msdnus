using System;
using System.Threading.Tasks;
using Lex.Db;

namespace msdnus.Core.Model
{
    public class DataTarget: IDataTarget
    {
        private static YourKey InitYourKey()
        {
            YourKey yk = new YourKey
            {
                Items = new YourKeyProduct_Key[]
                {
                    new YourKeyProduct_Key()
                    {
                        Key = new YourKeyProduct_KeyKey[]
                        {
                            new YourKeyProduct_KeyKey()
                            {
                                ClaimedDate = "2015-01-01",
                                ID = "ID1",
                                Type = "TYPE1",
                                Value = "VALUE1"
                            },
                            new YourKeyProduct_KeyKey()
                            {
                                ClaimedDate = "2015-01-02",
                                ID = "ID2",
                                Type = "TYPE2",
                                Value = "VALUE2"
                            },
                        },
                        Name = "TEST1"
                    }
                }
            };
            return yk;
        }

        public async Task<bool> SaveData(YourKey yk)
        {
            using (var db = new DbInstance("msdnus_db"))
            {
                try
                {
                    db.Map<YourKeyProduct_KeyKey>().Automap(i => i.Id);
                    db.Initialize();

                    foreach (var zz in InitYourKey().Items)
                    {
                        db.Save(zz.Key);
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
