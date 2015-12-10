using System.Xml.Serialization;

// ReSharper disable InconsistentNaming
//TODO: Refactor YourKey
namespace msdnus.Core.Model
{
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class YourKey
    {

        private YourKeyProduct_Key[] itemsField;

        [XmlElement("Product_Key", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public YourKeyProduct_Key[] Items
        {
            get { return itemsField; }
            set { itemsField = value; }
        }
    }

    [XmlType(AnonymousType = true)]
    public class YourKeyProduct_Key
    {

        private YourKeyProduct_KeyKey[] keyField;

        private string nameField;

        /// <remarks/>
        [XmlElement("Key", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
        public YourKeyProduct_KeyKey[] Key
        {
            get { return keyField; }
            set { keyField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public string Name
        {
            get { return nameField; }
            set { nameField = value; }
        }
    }

    [XmlType(AnonymousType = true)]
    public class YourKeyProduct_KeyKey
    {

        private string idField;

        private string typeField;

        private string claimedDateField;

        private string valueField;

        /// <remarks/>
        [XmlAttribute]
        public string ID
        {
            get { return idField; }
            set { idField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public string Type
        {
            get { return typeField; }
            set { typeField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public string ClaimedDate
        {
            get { return claimedDateField; }
            set { claimedDateField = value; }
        }

        /// <remarks/>
        [XmlText]
        public string Value
        {
            get { return valueField; }
            set { valueField = value; }
        }
    }
}