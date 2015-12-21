using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using GalaSoft.MvvmLight.Ioc;
using msdnus.Core.Model;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace msdnus.Tests.Model
{
    [TestClass]
    public class ModelTests
    {
        const string FNAME = @"KeysExport.xml";

        private static IDataSource _datasource;
        private static IDataTarget _datatarget;

        private static YourKey _dataFromArray;
        private static YourKey _yk;

        ModelTests()
        {
            InitDatasources();
        }

        [ClassInitialize]
        public static void ClassInit(TestContext ctx)
        {
            Lex.Db.Serialization.Extender.RegisterType<YourKeyKeyDTO, CustomTypesSerializers>(1000);
        }

        #region Init
        public static void InitDatasources()
        {
            SimpleIoc.Default.Register<IDataSource, FileDataSource>();
            SimpleIoc.Default.Register<IDataTarget, DataTarget>();
            _datasource = SimpleIoc.Default.GetInstance<IDataSource>();
            _datatarget = SimpleIoc.Default.GetInstance<IDataTarget>();

            _dataFromArray = InitYourKey();
            _yk = PrepareFileInLocalFolder().Result;
        }

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

        private static async Task<YourKey> PrepareFileInLocalFolder()
        {
            var bl = _datasource.SaveData(_dataFromArray, FNAME).Result;
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile file = await folder.GetFileAsync(FNAME);

            return await _datasource.GetData(file);
        }
        #endregion

        [TestMethod]
        public void ShouldFileWriteReadYourKey()
        {
            Assert.IsNotNull(_yk);
            Assert.AreEqual(_yk.Items.Length, _dataFromArray.Items.Length);
            Assert.AreEqual(_yk.Items[0].Name, _dataFromArray.Items[0].Name);

            Assert.AreEqual(_yk.Items[0].Key.Length, _dataFromArray.Items[0].Key.Length);

            foreach (var zzz in _yk.Items[0].Key)
            {
                bool isok =
                    _dataFromArray.Items[0].Key.Any(
                        yyy =>
                            (zzz.ClaimedDate == yyy.ClaimedDate) && (zzz.ID == yyy.ID) &&
                            (zzz.Type == yyy.Type) && (zzz.Value == yyy.Value));
                Assert.IsTrue(isok, zzz.ToString());
            }
        }

        [TestMethod]
        public void ShouldSaveYourKeyToDb()
        {
            //StorageFolder folder = ApplicationData.Current.LocalFolder;
            List<YourKeyDTO> lst = new List<YourKeyDTO>();

            foreach (var ff in _yk.Items)
            {
                YourKeyDTO dt = new YourKeyDTO();
                dt.MapFromYourKeyProduct_Key(ff);
                lst.Add(dt);
            }
            
            var isok = _datatarget.SaveData(lst, true);
            
            Assert.IsTrue(isok);
        }

        [TestMethod]
        public void ShoudLoadAllDataFromDb()
        {
            ShouldSaveYourKeyToDb();
            var zz = _datatarget.GetAllData().Result;
            Assert.IsNotNull(zz);
            Assert.IsTrue(zz.Count>0);
        }
    }
}
