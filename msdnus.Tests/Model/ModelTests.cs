using GalaSoft.MvvmLight.Ioc;
using msdnus.Core.Model;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace msdnus.Tests.Model
{
    [TestClass]
    public class ModelTests
    {
        [TestMethod] public void ShouldReadXmlFileToVar()
        {
            SimpleIoc.Default.Register<IDataSource, FileDataSource>();

            var datasource = SimpleIoc.Default.GetInstance<IDataSource>();

            YourKey zz = datasource.GetData(@"KeysExport.xml").Result;

            Assert.IsNotNull(zz);
            Assert.IsTrue(zz.Items.Length>0);
        }
    }
}
