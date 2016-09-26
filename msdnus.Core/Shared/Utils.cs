using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;

namespace msdnus.Core.Shared
{
    public static class Utils
    {
        private const uint BKEYLENGTH = 32;
        private const string CDRUSKA = "1908B56935BF";

        private static string _priejEilute = null;
        private static string PriejEilute
        {
            get
            {
                return _priejEilute ?? (_priejEilute = (NetworkInformation.GetHostNames()
                    .FirstOrDefault(name => name.Type == HostNameType.DomainName)?.DisplayName ?? CDRUSKA) + CDRUSKA);
            }
        }

        private static CryptographicKey _preparedRaktas = null;

        private static CryptographicKey PreparedRaktas => _preparedRaktas ?? (_preparedRaktas = PrepareKey());

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
            var serializer = new XmlSerializer(typeof(T));
            var objectFromXml = (T)serializer.Deserialize(stream);
            return objectFromXml;
        }

        public static async Task SaveObjectToXmlAsync<T>(T objectToSave, StorageFile file)
        {
            var serializer = new XmlSerializer(typeof(T));
            Stream stream = await file.OpenStreamForWriteAsync();

            using (stream)
            {
                serializer.Serialize(stream, objectToSave);
            }
        }

        /// <summary>
        /// ProtectToBase64Async, null if unsuccesful
        /// </summary>
        /// <param name="astrMsg">String to encrypt</param>
        /// <param name="strDescriptor">Descriptor</param>
        /// <returns>encrypted string or null</returns>
        public static string ProtectToBase64Async(string astrMsg)
        {

            return AES_EncryptToBase64Data(astrMsg);
        }

        /// <summary>
        /// UnprotectFromBase64DataAsync. Return null if uncuccessful
        /// </summary>
        /// <param name="astrBase64Protected">String to decrypt</param>
        /// <returns>Decrypted string. null if uncuccessful</returns>
        public static string UnprotectFromBase64DataAsync(string astrBase64Protected)
        {
            return AES_DecryptFromBase64Data(astrBase64Protected);
        }

        private static CryptographicKey PrepareKey()
        {
            SymmetricKeyAlgorithmProvider sap =
                SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesEcbPkcs7);
            HashAlgorithmProvider hap = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha512);
            CryptographicHash hashAes = hap.CreateHash();

            byte[] hash = new byte[BKEYLENGTH];
            hashAes.Append(CryptographicBuffer.CreateFromByteArray(Encoding.UTF8.GetBytes(PriejEilute)));
            byte[] temp;
            CryptographicBuffer.CopyToByteArray(hashAes.GetValueAndReset(), out temp);

            Array.Copy(temp, 0, hash, 0, 16);
            Array.Copy(temp, 0, hash, 15, 16);

            var aes = sap.CreateSymmetricKey(CryptographicBuffer.CreateFromByteArray(hash));
            return aes;
        }

        private static string AES_EncryptToBase64Data(string input)
        {
            try
            {
                if (string.IsNullOrEmpty(input)) return null;

                var aes = PreparedRaktas;

                IBuffer buffer = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);
                var rezenc = CryptographicEngine.Encrypt(aes, buffer, null);
                var encrypted = CryptographicBuffer.EncodeToBase64String(rezenc);

                return encrypted;
            }
            catch (Exception ex)
            {
#if DEBUG
                throw new Exception($"Enc: {ex.Message}");
#else
                return null;
#endif
            }
        }

        private static string AES_DecryptFromBase64Data(string input)
        {
            try
            {
                if (string.IsNullOrEmpty(input)) return null;

                var aes = PreparedRaktas;

                IBuffer buffer = CryptographicBuffer.DecodeFromBase64String(input);
                var rezdec = CryptographicEngine.Decrypt(aes, buffer, null);
                return CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, rezdec);
            }
            catch (Exception ex)
            {
#if DEBUG
                throw new Exception($"Dec: {ex.Message}");
#else
                return null;
#endif
            }
        }
    }
}
