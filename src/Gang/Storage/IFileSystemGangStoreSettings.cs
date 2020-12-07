namespace Gang.Storage
{
    public interface IFileSystemGangStoreSettings
    {
        string RootPath { get; }
        string FileExtension { get; }
        int CacheExpiryMins { get; }
    }
}
