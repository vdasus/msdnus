namespace msdnus.Core.Model
{
    //https://msdn.microsoft.com/en-us/subscriptions/securejson/GetAllExportKeys?brand=MSDN

    public interface IDataSource
    {
        root GetData(Stream stream);
    }
}
