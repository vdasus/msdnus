using System.Security.Cryptography;
using System.Text;

namespace msdnus.Core.Shared
{
    /// <summary>
    /// AES-GCM encryption with a machine-bound key. The key is derived from the machine name
    /// plus a fixed application salt, so protected data cannot be decrypted on another machine.
    /// </summary>
    public sealed class AesGcmCryptoService : ICryptoService
    {
        private const string AppSalt = "1908B56935BF-msdnus-v2";
        private const int KeyLength = 32;
        private const int NonceLength = 12;
        private const int TagLength = 16;
        private const int Pbkdf2Iterations = 100_000;

        private readonly byte[] _key;

        /// <param name="keySeed">
        /// Key material seed. Defaults to the machine name, which binds the key to this machine.
        /// Tests may pass a fixed seed.
        /// </param>
        public AesGcmCryptoService(string? keySeed = null)
        {
            var seed = string.IsNullOrEmpty(keySeed) ? Environment.MachineName : keySeed;
            _key = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(seed + AppSalt),
                Encoding.UTF8.GetBytes(AppSalt),
                Pbkdf2Iterations,
                HashAlgorithmName.SHA256,
                KeyLength);
        }

        public string? Protect(string? plain)
        {
            if (string.IsNullOrEmpty(plain)) return null;

            try
            {
                var plainBytes = Encoding.UTF8.GetBytes(plain);
                var nonce = RandomNumberGenerator.GetBytes(NonceLength);
                var cipher = new byte[plainBytes.Length];
                var tag = new byte[TagLength];

                using var aes = new AesGcm(_key, TagLength);
                aes.Encrypt(nonce, plainBytes, cipher, tag);

                var payload = new byte[NonceLength + TagLength + cipher.Length];
                nonce.CopyTo(payload, 0);
                tag.CopyTo(payload, NonceLength);
                cipher.CopyTo(payload, NonceLength + TagLength);
                return Convert.ToBase64String(payload);
            }
            catch (CryptographicException)
            {
                return null;
            }
        }

        public string? Unprotect(string? protectedBase64)
        {
            if (string.IsNullOrEmpty(protectedBase64)) return null;

            try
            {
                var payload = Convert.FromBase64String(protectedBase64);
                if (payload.Length < NonceLength + TagLength) return null;

                var nonce = payload.AsSpan(0, NonceLength);
                var tag = payload.AsSpan(NonceLength, TagLength);
                var cipher = payload.AsSpan(NonceLength + TagLength);
                var plain = new byte[cipher.Length];

                using var aes = new AesGcm(_key, TagLength);
                aes.Decrypt(nonce, cipher, tag, plain);
                return Encoding.UTF8.GetString(plain);
            }
            catch (FormatException)
            {
                return null;
            }
            catch (CryptographicException)
            {
                return null;
            }
        }

        public string DeriveSecret(string purpose)
        {
            var derived = Rfc2898DeriveBytes.Pbkdf2(
                _key,
                Encoding.UTF8.GetBytes(purpose + AppSalt),
                1,
                HashAlgorithmName.SHA256,
                KeyLength);
            return Convert.ToBase64String(derived);
        }
    }
}
