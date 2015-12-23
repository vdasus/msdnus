using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage;
using Windows.Storage.Streams;

namespace msdnus.Core.Shared
{
    public static class Utils
    {
        public static async Task<T> ReadObjectFromXmlFileAsync<T>(StorageFile file)
        {
            using (Stream stream = await file.OpenStreamForReadAsync())
            {
                var rez = ReadObjectFromXmlStreamAsync<T>(stream);
                return rez;
            }
               
        }

        public static T ReadObjectFromXmlStreamAsync<T>(Stream stream)
        {
            var serializer = new XmlSerializer(typeof (T));
            var objectFromXml = (T) serializer.Deserialize(stream);
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

        public static async Task<string> ProtectToBase64Async(string strMsg, string strDescriptor = "LOCAL=user")
        {
            if (string.IsNullOrEmpty(strMsg)) return null;
            const BinaryStringEncoding ENCODING = BinaryStringEncoding.Utf8;
            DataProtectionProvider provider = new DataProtectionProvider(strDescriptor);

            IBuffer buffMsg = CryptographicBuffer.ConvertStringToBinary(strMsg, ENCODING);
            IBuffer buffProtected = await provider.ProtectAsync(buffMsg);
            return CryptographicBuffer.EncodeToBase64String(buffProtected);
        }

        public static async Task<string> UnprotectFromBase64Data(string strBase64Protected)
        {
            if (string.IsNullOrEmpty(strBase64Protected)) return null;

            const BinaryStringEncoding ENCODING = BinaryStringEncoding.Utf8;
            DataProtectionProvider provider = new DataProtectionProvider();

            IBuffer tmpstr = CryptographicBuffer.DecodeFromBase64String(strBase64Protected);
            IBuffer buffUnprotected = await provider.UnprotectAsync(tmpstr);

            return CryptographicBuffer.ConvertBinaryToString(ENCODING, buffUnprotected);
        }
    }
}
