using GalaSoft.MvvmLight.Ioc;
using msdnus.Core.Model;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace msdnus.Tests.Model
{
    [TestClass]
    public class ModelTests
    {
        #region Init
        private static YourKey InitYourKey()
        {
            YourKey yk = new YourKey
            {
                Items = new YourKeyProduct_Key[]
                {
                    new YourKeyProduct_Key()
                    {
                        Key = new YourKeyProduct_KeyKey[]
                        {
                            new YourKeyProduct_KeyKey()
                            {
                                ClaimedDate = "2015-01-01",
                                ID = "ID1",
                                Type = "TYPE1",
                                Value = "VALUE1"
                            },
                            new YourKeyProduct_KeyKey()
                            {
                                ClaimedDate = "2015-01-02",
                                ID = "ID2",
                                Type = "TYPE2",
                                Value = "VALUE2"
                            },
                        },
                        Name = "TEST1"
                    }
                }
            };
            return yk;
        }
        #endregion

        [TestMethod]
        public void ShouldReadXmlFileToVar()
        {
            const string FNAME = @"KeysExport.xml";

            SimpleIoc.Default.Register<IDataSource, FileDataSource>();
            var datasource = SimpleIoc.Default.GetInstance<IDataSource>();

            var yk = InitYourKey();

            var isokwrite = datasource.SaveData(yk, FNAME).Result;

            YourKey zz = datasource.GetData(FNAME).Result;

            Assert.IsTrue(isokwrite);
            Assert.IsNotNull(zz);
            Assert.AreEqual(1, zz.Items.Length);
            Assert.AreEqual("TEST1", zz.Items[0].Name);

            Assert.AreEqual(2,zz.Items[0].Key.Length);

            Assert.AreEqual("2015-01-01", zz.Items[0].Key[0].ClaimedDate);
            Assert.AreEqual("ID1", zz.Items[0].Key[0].ID);
            Assert.AreEqual("TYPE1", zz.Items[0].Key[0].Type);
            Assert.AreEqual("VALUE1", zz.Items[0].Key[0].Value);

            Assert.AreEqual("2015-01-02", zz.Items[0].Key[1].ClaimedDate);
            Assert.AreEqual("ID2", zz.Items[0].Key[1].ID);
            Assert.AreEqual("TYPE2", zz.Items[0].Key[1].Type);
            Assert.AreEqual("VALUE2", zz.Items[0].Key[1].Value);
        }
    }
}
