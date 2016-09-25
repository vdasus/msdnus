using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage;
using Windows.Storage.Streams;

namespace msdnus.Core.Shared
{
    public static class Utils
    {
        private const BinaryStringEncoding BSENCODING = BinaryStringEncoding.Utf8;
        private const uint BKEYLENGTH = 32;

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
        public static async Task<string> ProtectToBase64Async(string astrMsg, string strDescriptor = "LOCAL=machine")
        {
            try
            {
                var strMsg = string.IsNullOrEmpty(astrMsg) ? "-" : astrMsg;

                DataProtectionProvider provider = new DataProtectionProvider(strDescriptor);

                IBuffer buffMsg = CryptographicBuffer.ConvertStringToBinary(strMsg, BSENCODING);
                IBuffer buffProtected = await provider.ProtectAsync(buffMsg);
                return CryptographicBuffer.EncodeToBase64String(buffProtected);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// UnprotectFromBase64DataAsync. Return null if uncuccessful
        /// </summary>
        /// <param name="astrBase64Protected">String to decrypt</param>
        /// <returns>Decrypted string. null if uncuccessful</returns>
        public static async Task<string> UnprotectFromBase64DataAsync(string astrBase64Protected)
        {
            try
            {
                var strBase64Protected = string.IsNullOrEmpty(astrBase64Protected) ? "-" : astrBase64Protected;

                if (string.IsNullOrEmpty(strBase64Protected)) return string.Empty;

                DataProtectionProvider provider = new DataProtectionProvider();

                IBuffer tmpstr = CryptographicBuffer.DecodeFromBase64String(strBase64Protected);
                IBuffer buffUnprotected = await provider.UnprotectAsync(tmpstr);
                return CryptographicBuffer.ConvertBinaryToString(BSENCODING, buffUnprotected);
            }
            catch
            {
                return null;
            }
        }

        public static string AES_Encrypt(string input, string pass)
        {
            SymmetricKeyAlgorithmProvider sap =
                SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesEcbPkcs7);
            HashAlgorithmProvider hap = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha512);
            CryptographicHash hashAes = hap.CreateHash();

            try
            {
                byte[] hash = new byte[32];
                hashAes.Append(CryptographicBuffer.CreateFromByteArray(Encoding.UTF8.GetBytes(pass)));
                byte[] temp;
                CryptographicBuffer.CopyToByteArray(hashAes.GetValueAndReset(), out temp);

                Array.Copy(temp, 0, hash, 0, 16);
                Array.Copy(temp, 0, hash, 15, 16);

                var aes = sap.CreateSymmetricKey(CryptographicBuffer.CreateFromByteArray(hash));

                IBuffer buffer = CryptographicBuffer.CreateFromByteArray(Encoding.UTF8.GetBytes(input));
                var encrypted = CryptographicBuffer.EncodeToBase64String(CryptographicEngine.Encrypt(aes, buffer, null));

                return encrypted;
            }
            catch (Exception)
            {
                return null;
            }
        }


        public static string AES_Decrypt(string input, string pass)
        {
            SymmetricKeyAlgorithmProvider sap = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesEcbPkcs7);
            HashAlgorithmProvider hap = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha512);
            CryptographicHash hashAes = hap.CreateHash();

            try
            {
                byte[] hash = new byte[32];
                hashAes.Append(CryptographicBuffer.CreateFromByteArray(Encoding.UTF8.GetBytes(pass)));
                byte[] temp;
                CryptographicBuffer.CopyToByteArray(hashAes.GetValueAndReset(), out temp);

                Array.Copy(temp, 0, hash, 0, 16);
                Array.Copy(temp, 0, hash, 15, 16);

                var aes = sap.CreateSymmetricKey(CryptographicBuffer.CreateFromByteArray(hash));

                IBuffer buffer = CryptographicBuffer.DecodeFromBase64String(input);
                byte[] bdecrypted;
                CryptographicBuffer.CopyToByteArray(CryptographicEngine.Decrypt(aes, buffer, null), out bdecrypted);
                var decrypted = Encoding.UTF8.GetString(bdecrypted, 0, bdecrypted.Length);

                return decrypted;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
