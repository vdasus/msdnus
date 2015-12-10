using System.Collections.Generic;
using Lex.Db.Serialization;

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
            Value = ykk.Value;
        }
    }

    public class CustomTypesSerializers
    {
        public static YourKeyKeyDTO ReadYourKeyKeyDTO(DataReader reader)
        {
            return new YourKeyKeyDTO
            {
                Id = reader.ReadInt32(),
                ID = reader.ReadString(),
                Type = reader.ReadString(),
                Value = reader.ReadString(),
                ClaimedDate = reader.ReadString()
            };
        }

        public static void WriteYourKeyKeyDTO(DataWriter writer, YourKeyKeyDTO value)
        {
            writer.Write(value.Id);
            writer.Write(value.ID);
            writer.Write(value.Type);
            writer.Write(value.Value);
            writer.Write(value.ClaimedDate);
        }
    }
}
