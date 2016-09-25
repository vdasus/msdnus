using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using GalaSoft.MvvmLight.Ioc;
using msdnus.Core.Model;
using Xunit;

namespace msdnus.Tests.Model
{
    public class ModelTests
    {
        private const string FNAME = @"KeysExport.xml";

        private static StorageFile _file;

        private static IDataSource _datasource;
        private static IDataTarget _datatarget;

        private static YourKey _dataFromArray;
        private static YourKey _yk;

        public ModelTests()
        {
            InitDatasources();
            Lex.Db.Serialization.Extender.RegisterType<YourKeyKeyDTO, CustomTypesSerializers>(1000);
        }

        ~ModelTests()
        {
            var deleteAsync = _file.DeleteAsync(StorageDeleteOption.PermanentDelete);
        }

        #region Init
        public static void InitDatasources()
        {
            SimpleIoc.Default.Register<IDataSource, FileDataSource>();
            SimpleIoc.Default.Register<IDataTarget, DataTarget>();
            _datasource = SimpleIoc.Default.GetInstance<IDataSource>();
            _datatarget = SimpleIoc.Default.GetInstance<IDataTarget>();

            _dataFromArray = InitYourKey();
            _yk = PrepareFileInLocalFolderAsync().Result;
        }

        private static YourKey InitYourKey()
        {
            YourKey yk = new YourKey
            {
                Items = new[]
                {
                    new YourKeyProduct_Key()
                    {
                        Key = new[]
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

        private static async Task<YourKey> PrepareFileInLocalFolderAsync()
        {
            var bl = _datasource.SaveDataAsync(_dataFromArray, FNAME).Result;
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            _file = await folder.GetFileAsync(FNAME);

            return await _datasource.GetDataAsync(_file);
        }
        #endregion

        [Fact]
        public void ShouldFileWriteReadYourKey()
        {
            Assert.NotNull(_yk);
            Assert.Equal(_yk.Items.Length, _dataFromArray.Items.Length);
            Assert.Equal(_yk.Items[0].Name, _dataFromArray.Items[0].Name);

            Assert.Equal(_yk.Items[0].Key.Length, _dataFromArray.Items[0].Key.Length);

            foreach (var zzz in _yk.Items[0].Key)
            {
                bool isok =
                    _dataFromArray.Items[0].Key.Any(
                        yyy =>
                            (zzz.ClaimedDate == yyy.ClaimedDate) && (zzz.ID == yyy.ID) &&
                            (zzz.Type == yyy.Type) && (zzz.Value == yyy.Value));
                Assert.True(isok, zzz.ToString());
            }
        }

        [Fact]
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
            
            var isok = _datatarget.SaveData(lst, true, true);
            
            Assert.True(isok);
        }

        [Fact]
        public void ShoudLoadAllDataFromDb()
        {
            ShouldSaveYourKeyToDb();
            var zz = _datatarget.GetAllDataAsync().Result;
            Assert.NotNull(zz);
            Assert.True(zz.Count>0);
        }
    }
}
