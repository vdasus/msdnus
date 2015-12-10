using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;

namespace msdnus.Core.Shared
{
    public static class Utils
    {
        public static async Task<T> ReadObjectFromXmlFileAsync<T>(StorageFile file)
        {
            var serializer = new XmlSerializer(typeof (T));

            Stream stream = await file.OpenStreamForReadAsync();
            var objectFromXml = (T) serializer.Deserialize(stream);
            stream.Dispose();
            return objectFromXml;
        }
        
        public static async Task SaveObjectToXmlAsync<T>(T objectToSave, StorageFile file)
        {
            var serializer = new XmlSerializer(typeof (T));
            Stream stream = await file.OpenStreamForWriteAsync();

            using (stream)
            {
                serializer.Serialize(stream, objectToSave);
            }
        }
    }
}
