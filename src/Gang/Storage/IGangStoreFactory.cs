namespace Gang.Storage
{
    /// <summary>
    /// Inject to create a store for data with indexes
    /// </summary>
    public interface IGangStoreFactory
    {
        /// <summary>
        /// Create a store for the given data type
        /// </summary>
        GangStoreFactory<TData> For<TData>();
    }
}
