using Windows.Storage;
//using msdnus.Model;
//using msdnus.Shared;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace msdnus.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            Assert.IsFalse(string.IsNullOrEmpty(folder.Name));
            //YourKey zz = Utils.ReadObjectFromXmlFileAsync<YourKey>(@"KeysExport.xml").Result;
            //Assert.IsTrue(zz.Items.Length>0);
        }
    }
}
