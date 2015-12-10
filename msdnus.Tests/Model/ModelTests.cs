using System.Linq;
using GalaSoft.MvvmLight.Ioc;
using msdnus.Core.Model;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace msdnus.Tests.Model
{
    [TestClass]
    public class ModelTests
    {
        private IDataSource datasource;
        private IDataTarget datatarget;

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

        private void InitDatasources()
        {
            SimpleIoc.Default.Register<IDataSource, FileDataSource>();
            SimpleIoc.Default.Register<IDataTarget, DataTarget>();
            datasource = SimpleIoc.Default.GetInstance<IDataSource>();
            datatarget = SimpleIoc.Default.GetInstance<IDataTarget>();
        }
        #endregion

        [TestMethod]
        public void ShouldFileWriteReadYourKey()
        {
            const string FNAME = @"KeysExport.xml";

            InitDatasources();

            var yk = InitYourKey();

            var isokwrite = datasource.SaveData(yk, FNAME).Result;

            YourKey zz = datasource.GetData(FNAME).Result;

            Assert.IsTrue(isokwrite);
            Assert.IsNotNull(zz);
            Assert.AreEqual(yk.Items.Length, zz.Items.Length);
            Assert.AreEqual(yk.Items[0].Name, zz.Items[0].Name);

            Assert.AreEqual(yk.Items[0].Key.Length, zz.Items[0].Key.Length);

            foreach (var zzz in yk.Items[0].Key)
            {
                bool isok =
                    zz.Items[0].Key.Any(
                        yyy =>
                            (zzz.ClaimedDate == yyy.ClaimedDate) && (zzz.ID == yyy.ID) &&
                            (zzz.Type == yyy.Type) && (zzz.Value == yyy.Value));
                Assert.IsTrue(isok, zzz.ToString());
            }
        }

        [TestMethod]
        public void ShouldSaveYourKeyToDb()
        {
            
        }
    }
}
