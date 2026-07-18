using AutoFixture;
using FluentAssertions;
using msdnus.Core.Shared;
using Xunit;

namespace msdnus.Tests.Shared
{
    public class CryptoServiceTests
    {
        private readonly Fixture _fixture = new();
        private readonly AesGcmCryptoService _sut = new("fixed-test-seed");

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
        public void Unprotect_WithDifferentKeySeed_ReturnsNull()
        {
            var encrypted = _sut.Protect(_fixture.Create<string>());

            var otherMachine = new AesGcmCryptoService("another-seed");
            var decrypted = otherMachine.Unprotect(encrypted);

            decrypted.Should().BeNull();
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
            var db1 = _sut.DeriveSecret("msdnusdb");
            var db2 = _sut.DeriveSecret("msdnusdb");
            var other = _sut.DeriveSecret("other-purpose");

            db1.Should().Be(db2);
            db1.Should().NotBe(other);
        }
    }
}
