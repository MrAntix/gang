using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Gang.Storage
{
    /// <summary>
    /// Generic Store for data
    /// </summary>
    /// <example>
    /// Don't inject directly, use a <see cref="IGangStoreFactory{TData}"/> to create the
    /// store with the required indexes
    /// <code>
    ///     public GangStateStore(
    ///         IGangStoreFactory<SomeData> storeFactory
    ///         )
    ///     {
    ///         ...
    ///         _store = storeFactory
    ///             .AddIndex(e => e.ImportantProperty)
    ///             .AddIndex(e => e.OtherImportantProperty)
    ///             .Create();
    ///     }
    /// </code>
    /// </example>
    /// <typeparam name="TData"></typeparam>
    public interface IGangStore<TData>
    {
        /// <summary>
        /// Get keys stored for the indexed value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="skip">Skip</param>
        /// <param name="take">Take</param>
        Task<IImmutableList<string>> TryGetIndexedKeys(object value, int skip = 0, int? take = null);

        /// <summary>
        /// Put data by key, indexes will run
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="data">Data</param>
        Task PutAsync(string key, TData data);

        /// <summary>
        /// Get data by key
        /// </summary>
        /// <param name="key">Key</param>
        Task<TData> TryGetAsync(string key);

        /// <summary>
        /// Delete data by key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>True if data is deleted</returns>
        Task<bool> TryDeleteAsync(string key);
    }
}
