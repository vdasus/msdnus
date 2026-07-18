using System.Xml.Serialization;

namespace msdnus.Core.Shared
{
    public static class Utils
    {
        public static T ReadObjectFromXmlStream<T>(Stream stream)
        {
            var serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(stream)!;
        }

        public static void SaveObjectToXmlStream<T>(T objectToSave, Stream stream)
        {
            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(stream, objectToSave);
        }
    }
}
