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
        /// <remarks/>
        [XmlElement("Key", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
        public YourKeyProduct_KeyKey[] Key { get; set; }

        /// <remarks/>
        [XmlAttribute]
        public string Name { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class YourKeyProduct_KeyKey
    {
        /// <remarks/>
        [XmlAttribute]
        public string ID { get; set; }

        /// <remarks/>
        [XmlAttribute]
        public string Type { get; set; }

        /// <remarks/>
        [XmlAttribute]
        public string ClaimedDate { get; set; }

        /// <remarks/>
        [XmlText]
        public string Value { get; set; }
    }
}