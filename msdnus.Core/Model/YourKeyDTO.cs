namespace msdnus.Core.Model
{
    public class YourKeyDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public List<YourKeyKeyDTO> Items { get; set; } = new();

        public void MapFromYourKeyProduct_Key(rootYourKeyProduct_Key yk)
        {
            Name = yk.Name ?? string.Empty;
            if (yk.Key == null) return;

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
        public bool Equals(YourKeyDTO? g1, YourKeyDTO? g2)
        {
            if (ReferenceEquals(g1, g2)) return true;
            if (g1 is null || g2 is null) return false;
            int cnt = (from zz in g1.Items from yy in g2.Items where zz.Value == yy.Value select zz).Count();
            return (g1.Name == g2.Name) && (cnt == g1.Items.Count);
        }

        public int GetHashCode(YourKeyDTO obj)
        {
            int sum = obj.Items.Aggregate(0, (current, zz) => current ^ (zz.Value ?? string.Empty).GetHashCode());
            return (obj.Name ?? string.Empty).GetHashCode() ^ sum;
        }
    }

    public class YourKeyKeyDTO
    {
        public int Id { get; set; }

        public string ID { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string ClaimedDate { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public void MapFromYourKeyProduct_KeyKey(rootYourKeyProduct_KeyKey ykk)
        {
            // Empty XML elements/attributes deserialize as null — never let that
            // propagate into the DTO (comparer and UI expect non-null strings).
            ID = ykk.ID ?? string.Empty;
            Type = ykk.Type ?? string.Empty;
            ClaimedDate = ykk.ClaimedDate ?? string.Empty;
            Value = ykk.Value ?? string.Empty;
        }
    }
}
