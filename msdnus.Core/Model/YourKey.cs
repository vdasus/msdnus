using System.Xml.Serialization;

// ReSharper disable InconsistentNaming
namespace msdnus.Core.Model
{
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class YourKey
    {
        [XmlElement("Product_Key", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public YourKeyProduct_Key[] Items { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class YourKeyProduct_Key
    {
        [XmlElement("Key", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
        public YourKeyProduct_KeyKey[] Key { get; set; }

        [XmlAttribute]
        public string Name { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class YourKeyProduct_KeyKey
    {
        [XmlAttribute]
        public string ID { get; set; }

        [XmlAttribute]
        public string Type { get; set; }

        [XmlAttribute]
        public string ClaimedDate { get; set; }

        [XmlText]
        public string Value { get; set; }

        [XmlIgnore]
        public string ValueFormatted => Value.Length < 50 ? Value : "-";
    }
}