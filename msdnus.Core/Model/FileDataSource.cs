using msdnus.Core.Shared;

namespace msdnus.Core.Model
{
    public class FileDataSource : IDataSource
    {
        public root GetData(Stream stream)
        {
            return Utils.ReadObjectFromXmlStream<root>(stream);
        }
    }
}
