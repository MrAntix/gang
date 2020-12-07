using System;
using System.Linq;
using System.Threading.Tasks;

namespace Gang.Storage
{
    public static class GangStoreExtensions
    {
        public static GangStoreFactory<TData> AddIndex<TData>(
            this GangStoreFactory<TData> store,
            Func<TData, object> indexer
            )
        {
            return store.AddIndex(
                d => new[] { indexer(d) }
                );
        }

        public static async Task<string> TryGetIndexedKeyAsync<TData>(
            this IGangStore<TData> store,
            object value
            )
        {
            var keys = await store
                .TryGetIndexedKeys(value);

            return keys.FirstOrDefault();
        }
    }
}
