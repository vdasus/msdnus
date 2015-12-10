using System;
using System.Threading.Tasks;
using Windows.Storage;
using msdnus.Core.Shared;

namespace msdnus.Core.Model
{
    public class FileDataSource: IDataSource
    {
        public async Task<YourKey> GetData(string source)
        {
            //Commented as snippets for reuse
            //StorageFolder folder = ApplicationData.Current.LocalFolder;
            //StorageFile file = await folder.GetFileAsync(filename);
            //if (!file.IsAvailable) file = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///{filename}"));

            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile file = await folder.GetFileAsync(source);
            return await Utils.ReadObjectFromXmlFileAsync<YourKey>(file);
        }

        public async Task<bool> SaveData<T>(T objectToSave, string source)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile file = await folder.CreateFileAsync(source, CreationCollisionOption.ReplaceExisting);
            await Utils.SaveObjectToXmlAsync<T>(objectToSave, file);
            return true;
        }
    }
}
