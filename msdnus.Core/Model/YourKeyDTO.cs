using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using msdnus.Core.Shared;
using DataReader = Lex.Db.Serialization.DataReader;
using DataWriter = Lex.Db.Serialization.DataWriter;

namespace msdnus.Core.Model
{
    public class YourKeyDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }
        
        public List<YourKeyKeyDTO> Items { get; set; }

        public YourKeyDTO()
        {
            Items = new List<YourKeyKeyDTO>();
        }

        public void MapFromYourKeyProduct_Key(YourKeyProduct_Key yk)
        {
            Name = yk.Name;
            foreach (var gg in yk.Key)
            {
                var tt = new YourKeyKeyDTO();
                tt.MapFromYourKeyProduct_KeyKey(gg);
                Items.Add(tt);
            }
        }
    }

    public class YourKeyDTOComparer : IEqualityComparer<YourKeyDTO>
    {
        public bool Equals(YourKeyDTO g1, YourKeyDTO g2)
        {
            int cnt = (from zz in g1.Items from yy in g2.Items where zz.Value == yy.Value select zz).Count();
            return (g1.Name == g2.Name) && (cnt == g1.Items.Count);
        }

        public int GetHashCode(YourKeyDTO obj)
        {
            int sum = obj.Items.Aggregate(0, (current, zz) => current ^ zz.Value.GetHashCode());
            return obj.Name.GetHashCode() ^ sum;
        }
    }

    public class YourKeyKeyDTO
    {
        public int Id { get; set; }

        public string ID { get; set; }

        public string Type { get; set; }

        public string ClaimedDate { get; set; }

        public string Value { get; set; }

        public void MapFromYourKeyProduct_KeyKey(YourKeyProduct_KeyKey ykk)
        {
            ID = ykk.ID;
            Type = ykk.Type;
            ClaimedDate = ykk.ClaimedDate;
            Value = ykk.ValueFormatted;
        }
    }

    public class CustomTypesSerializers
    {
        public static YourKeyKeyDTO ReadYourKeyKeyDTO(DataReader reader)
        {
            return Task.Run(async () => new YourKeyKeyDTO
            {
                Id = reader.ReadInt32(),
                ID = await Utils.UnprotectFromBase64DataAsync(reader.ReadString()),
                Type = await Utils.UnprotectFromBase64DataAsync(reader.ReadString()),
                Value = await Utils.UnprotectFromBase64DataAsync(reader.ReadString()),
                ClaimedDate = await Utils.UnprotectFromBase64DataAsync(reader.ReadString()),
            }).Result;
        }

        public static void WriteYourKeyKeyDTO(DataWriter writer, YourKeyKeyDTO value)
        {
            Task.Run(async () =>
            {
                writer.Write(value.Id);
                writer.Write(await Utils.ProtectToBase64Async(value.ID));
                writer.Write(await Utils.ProtectToBase64Async(value.Type));
                writer.Write(await Utils.ProtectToBase64Async(value.Value));
                writer.Write(await Utils.ProtectToBase64Async(value.ClaimedDate));
            }).Wait();
        }
    }
}
