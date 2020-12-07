using Gang.Storage;

namespace Gang.Demo.Web.Properties
{
    public sealed class FileStoreSettings :
        IFileSystemGangStoreSettings
    {
        public string RootPath { get; set; }
        public string FileExtension { get; set; } = ".json";
        public int CacheExpiryMins { get; set; } = 5;
    }
}
