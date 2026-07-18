using System.Text;
using FluentAssertions;
using msdnus.Core.Model;
using Xunit;

namespace msdnus.Tests.Model
{
    public class FileDataSourceTests
    {
        private const string SampleXml = """
            <?xml version="1.0" encoding="utf-8"?>
            <root>
              <YourSubscription>
                <Subscription />
              </YourSubscription>
              <YourKey>
                <Product_Key Name="Visual Studio" KeyRetrievalNote="note">
                  <Key ID="42" Type="Static Activation Key" ClaimedDate="2015-01-01">AAAAA-BBBBB-CCCCC</Key>
                  <Key ID="43" Type="Static Activation Key" ClaimedDate="2015-01-02">DDDDD-EEEEE-FFFFF</Key>
                </Product_Key>
                <Product_Key Name="Office 365" KeyRetrievalNote="note" />
              </YourKey>
            </root>
            """;

        [Fact]
        [Trait("Category", "SmokeTest")]
        [Trait("Priority", "High")]
        public void GetData_ParsesMsdnKeysExportXml()
        {
            var sut = new FileDataSource();
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SampleXml));

            var data = sut.GetData(stream);

            var yourKey = data.Items.OfType<rootYourKey>().Single();
            yourKey.Product_Key.Should().HaveCount(2);

            var vs = yourKey.Product_Key[0];
            vs.Name.Should().Be("Visual Studio");
            vs.Key.Should().HaveCount(2);
            vs.Key[0].ID.Should().Be("42");
            vs.Key[0].Value.Should().Be("AAAAA-BBBBB-CCCCC");
            vs.Key[0].ClaimedDate.Should().Be("2015-01-01");
        }

        [Fact]
        [Trait("Category", "UnitTest")]
        [Trait("Priority", "High")]
        public void GetData_MappedToDto_PreservesKeyData()
        {
            var sut = new FileDataSource();
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SampleXml));
            var data = sut.GetData(stream);
            var yourKey = data.Items.OfType<rootYourKey>().Single();

            var dto = new YourKeyDTO();
            dto.MapFromYourKeyProduct_Key(yourKey.Product_Key[0]);

            dto.Name.Should().Be("Visual Studio");
            dto.Items.Should().HaveCount(2);
            dto.Items[0].Value.Should().Be("AAAAA-BBBBB-CCCCC");
            dto.Items[0].Type.Should().Be("Static Activation Key");
        }
    }
}
