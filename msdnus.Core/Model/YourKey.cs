using System.Xml.Serialization;

// ReSharper disable InconsistentNaming
namespace msdnus.Core.Model
{
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class YourKey
    {
        [XmlIgnore]
        public int Id { get; set; }

        [XmlElement("Product_Key", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public YourKeyProduct_Key[] Items { get; set; }

        public YourKey() { }
    }

    [XmlType(AnonymousType = true)]
    public class YourKeyProduct_Key
    {
        [XmlIgnore]
        public int Id { get; set; }

        [XmlElement("Key", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
        public YourKeyProduct_KeyKey[] Key { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        public YourKeyProduct_Key() { }
    }

    [XmlType(AnonymousType = true)]
    public class YourKeyProduct_KeyKey
    {
        [XmlIgnore]
        public int Id { get; set; }

        [XmlAttribute]
        public string ID { get; set; }

        [XmlAttribute]
        public string Type { get; set; }

        [XmlAttribute]
        public string ClaimedDate { get; set; }

        [XmlText]
        public string Value { get; set; }

        public YourKeyProduct_KeyKey() { }
    }
}