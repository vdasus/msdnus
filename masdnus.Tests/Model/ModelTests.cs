using Windows.ApplicationModel.DataTransfer;
using msdnus.Model;
using msdnus.Shared;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace masdnus.Tests.Model
{
    [TestClass]
    public class ModelTests
    {
        [TestMethod]
        public void ShouldCreateMsdnItem()
        {
            YourKey zz = Utils.ReadObjectFromXmlFileAsync<YourKey>(@"KeysExport.xml").Result;

        }
    }
}
