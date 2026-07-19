using System;
using System.Security.Cryptography;
using System.Text;

namespace msdnus.Core.Shared
{
    /// <summary>
    /// AES-GCM encryption with a key derived from a user PIN/passphrase and a random
    /// per-vault salt (PBKDF2-SHA256). The key is <b>not</b> bound to the machine, so an
    /// encrypted vault can be synced across machines (e.g. via Dropbox) and opened anywhere
    /// with the correct PIN. The PIN may contain any characters, not just digits.
    /// </summary>
    public sealed class AesGcmCryptoService : ICryptoService
    {
        /// <summary>Default PBKDF2 iteration count. High enough to make brute-force expensive.</summary>
        public const int DefaultIterations = 300_000;

        private const int KeyLength = 32;
        private const int NonceLength = 12;
        private const int TagLength = 16;

        private readonly byte[] _key;

        /// <param name="pin">User PIN or passphrase (any characters).</param>
        /// <param name="salt">Random per-vault salt (16+ bytes recommended). Not secret.</param>
        /// <param name="iterations">PBKDF2-SHA256 iteration count.</param>
        public AesGcmCryptoService(string pin, byte[] salt, int iterations = DefaultIterations)
        {
            if (salt is null || salt.Length == 0)
                throw new ArgumentException("A non-empty salt is required.", nameof(salt));

            _key = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(pin ?? string.Empty),
                salt,
                iterations,
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
            // Domain-separated one-way derivation from the master key. Used for the
            // per-profile database passwords and the PIN verifier — none of which
            // reveal the master key or each other.
            var derived = Rfc2898DeriveBytes.Pbkdf2(
                _key,
                Encoding.UTF8.GetBytes("msdnus:" + purpose),
                1,
                HashAlgorithmName.SHA256,
                KeyLength);
            return Convert.ToBase64String(derived);
        }
    }
}
