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

        public async Task<YourKey> GetDataAsync(StorageFile source)
        {
            return await Utils.ReadObjectFromXmlFileAsync<YourKey>(source);
        }

        public YourKey GetDataAsync(Stream stream)
        {
            return Utils.ReadObjectFromXmlStreamAsync<YourKey>(stream);
        }

        public async Task<bool> SaveDataAsync<T>(T objectToSave, string source)
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
