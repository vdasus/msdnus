using Windows.ApplicationModel.DataTransfer;
using msdnus.Model;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace masdnus.Tests.Model
{
    [TestClass]
    public class ModelTests
    {
        [TestMethod]
        public void ShouldCreateMsdnItem()
        {
            MsdnItem mi = new MsdnItem();
            var zz = msdnus.Shared.Utils.ReadObjectFromXmlFileAsync<MsdnItem>("aaa").Result;

        }
    }
}
