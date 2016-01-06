using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using msdnus.Core.Shared;

namespace msdnus.Core.Model
{
    public class FileDataSource: IDataSource
    {
        private bool _disposed;

        public async Task<YourKey> GetData(StorageFile source)
        {
            return await Utils.ReadObjectFromXmlFileAsync<YourKey>(source);
        }

        public YourKey GetData(Stream stream)
        {
            return Utils.ReadObjectFromXmlStreamAsync<YourKey>(stream);
        }

        public async Task<bool> SaveData<T>(T objectToSave, string source)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile file = await folder.CreateFileAsync(source, CreationCollisionOption.ReplaceExisting);
            await Utils.SaveObjectToXmlAsync(objectToSave, file);
            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                
            }

            _disposed = true;
        }

        ~FileDataSource()
        {
            Dispose(false);
        }
    }
}
