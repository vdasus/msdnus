using System.Text;
using AutoFixture;
using FluentAssertions;
using msdnus.Core.Shared;
using Xunit;

namespace msdnus.Tests.Shared
{
    public class CryptoServiceTests
    {
        // Low iteration count keeps the tests fast; production uses AesGcmCryptoService.DefaultIterations.
        private const int TestIterations = 1_000;

        private static readonly byte[] Salt = Encoding.UTF8.GetBytes("0123456789abcdef");
        private static readonly byte[] OtherSalt = Encoding.UTF8.GetBytes("fedcba9876543210");

        private readonly Fixture _fixture = new();
        private readonly AesGcmCryptoService _sut = new("correct-pin", Salt, TestIterations);

        [Fact]
        [Trait("Category", "SmokeTest")]
        [Trait("Priority", "High")]
        public void Protect_ThenUnprotect_RoundTripsPlaintext()
        {
            const string plain = "Just a test for encrypting {B5E5BE54-E428-4F24-AC80-83CA5EF8B4C0}";

            var encrypted = _sut.Protect(plain);
            var decrypted = _sut.Unprotect(encrypted);

            encrypted.Should().NotBeNullOrEmpty().And.NotBe(plain);
            decrypted.Should().Be(plain);
        }

        [Fact]
        [Trait("Category", "UnitTest")]
        [Trait("Priority", "High")]
        public void Unprotect_WithDifferentPin_ReturnsNull()
        {
            var encrypted = _sut.Protect(_fixture.Create<string>());

            var wrongPin = new AesGcmCryptoService("wrong-pin", Salt, TestIterations);
            var decrypted = wrongPin.Unprotect(encrypted);

            decrypted.Should().BeNull();
        }

        [Fact]
        [Trait("Category", "UnitTest")]
        [Trait("Priority", "High")]
        public void Unprotect_WithDifferentSalt_ReturnsNull()
        {
            var encrypted = _sut.Protect(_fixture.Create<string>());

            var differentVault = new AesGcmCryptoService("correct-pin", OtherSalt, TestIterations);
            var decrypted = differentVault.Unprotect(encrypted);

            decrypted.Should().BeNull("the salt is part of the derived key, so the same PIN yields a different key");
        }

        [Fact]
        [Trait("Category", "UnitTest")]
        [Trait("Priority", "High")]
        public void SamePinAndSalt_DeriveInteroperableKeys()
        {
            var encrypted = _sut.Protect("shared-secret");

            var otherMachine = new AesGcmCryptoService("correct-pin", Salt, TestIterations);
            otherMachine.Unprotect(encrypted).Should().Be("shared-secret",
                "the key is derived from PIN + salt only, so a synced vault opens on any machine");
        }

        [Fact]
        [Trait("Category", "UnitTest")]
        [Trait("Priority", "Medium")]
        public void Protect_SamePlaintextTwice_ProducesDifferentCiphertexts()
        {
            var plain = _fixture.Create<string>();

            var first = _sut.Protect(plain);
            var second = _sut.Protect(plain);

            first.Should().NotBe(second, "AES-GCM must use a fresh random nonce per encryption");
        }

        [Theory]
        [Trait("Category", "UnitTest")]
        [Trait("Priority", "Medium")]
        [InlineData(null)]
        [InlineData("")]
        public void Protect_EmptyInput_ReturnsNull(string? input)
        {
            _sut.Protect(input).Should().BeNull();
        }

        [Theory]
        [Trait("Category", "UnitTest")]
        [Trait("Priority", "Medium")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("not-base64!!!")]
        [InlineData("dG9vLXNob3J0")]
        public void Unprotect_InvalidInput_ReturnsNull(string? input)
        {
            _sut.Unprotect(input).Should().BeNull();
        }

        [Fact]
        [Trait("Category", "UnitTest")]
        [Trait("Priority", "Medium")]
        public void DeriveSecret_IsStablePerPurpose_AndDiffersBetweenPurposes()
        {
            var db1 = _sut.DeriveSecret("db:profile_a");
            var db2 = _sut.DeriveSecret("db:profile_a");
            var verify = _sut.DeriveSecret("pin-verify");

            db1.Should().Be(db2);
            db1.Should().NotBe(verify);
        }
    }
}
