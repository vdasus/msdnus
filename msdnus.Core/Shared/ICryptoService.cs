namespace msdnus.Core.Shared
{
    public interface ICryptoService
    {
        /// <summary>Encrypts a string and returns it as base64, or null on failure/empty input.</summary>
        string? Protect(string? plain);

        /// <summary>Decrypts a base64 string produced by <see cref="Protect"/>, or null on failure/empty input.</summary>
        string? Unprotect(string? protectedBase64);

        /// <summary>Derives a stable secret string for the given purpose (e.g. a database password).</summary>
        string DeriveSecret(string purpose);
    }
}
