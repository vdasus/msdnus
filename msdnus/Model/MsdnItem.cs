using System.Xml.Serialization;
// ReSharper disable InconsistentNaming

namespace msdnus.Model
{
    /*
    <Product_Key Name="Windows 8 Pro">
     <Key ID="920" Type="Retail" ClaimedDate="11/29/2015">XXXXX-XXXXX-XXXXX-XXXXX-XXXXX</Key>
     <Key ID="920" Type="Retail" ClaimedDate="11/29/2015">XXXXX-XXXXX-XXXXX-XXXXX-XXXXX</Key>
     <Key ID="920" Type="Retail" ClaimedDate="11/29/2015">XXXXX-XXXXX-XXXXX-XXXXX-XXXXX</Key>
     <Key ID="920" Type="Retail" ClaimedDate="7/28/2015">XXXXX-XXXXX-XXXXX-XXXXX-XXXXX</Key>
     <Key ID="920" Type="Retail" ClaimedDate="7/28/2015">XXXXX-XXXXX-XXXXX-XXXXX-XXXXX</Key>
    </Product_Key>
        */

    public class MsdnItem
    {

    }

    [XmlTypeAttribute(AnonymousType = true)]
    [XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class YourKey
    {

        private YourKeyProduct_Key[] itemsField;

        /// <remarks/>
        [XmlElementAttribute("Product_Key", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public YourKeyProduct_Key[] Items
        {
            get { return itemsField; }
            set { itemsField = value; }
        }
    }

    [XmlTypeAttribute(AnonymousType = true)]
    public class YourKeyProduct_Key
    {

        private YourKeyProduct_KeyKey[] keyField;

        private string nameField;

        /// <remarks/>
        [XmlElementAttribute("Key", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
        public YourKeyProduct_KeyKey[] Key
        {
            get { return keyField; }
            set { keyField = value; }
        }

        /// <remarks/>
        [XmlAttributeAttribute()]
        public string Name
        {
            get { return nameField; }
            set { nameField = value; }
        }
    }

    [XmlTypeAttribute(AnonymousType = true)]
    public class YourKeyProduct_KeyKey
    {

        private string idField;

        private string typeField;

        private string claimedDateField;

        private string valueField;

        /// <remarks/>
        [XmlAttributeAttribute()]
        public string ID
        {
            get { return idField; }
            set { idField = value; }
        }

        /// <remarks/>
        [XmlAttributeAttribute()]
        public string Type
        {
            get { return typeField; }
            set { typeField = value; }
        }

        /// <remarks/>
        [XmlAttributeAttribute()]
        public string ClaimedDate
        {
            get { return claimedDateField; }
            set { claimedDateField = value; }
        }

        /// <remarks/>
        [XmlTextAttribute()]
        public string Value
        {
            get { return valueField; }
            set { valueField = value; }
        }
    }
}
