using Gang.Serialization;
using Microsoft.Extensions.Caching.Memory;

namespace Gang.Storage
{
    public sealed class FileSystemGangStoreFactory :
        IGangStoreFactory
    {
        readonly IFileSystemGangStoreSettings _settings;
        readonly IGangSerializationService _serializer;
        readonly IMemoryCache _cache;

        public FileSystemGangStoreFactory(
            IFileSystemGangStoreSettings settings,
            IGangSerializationService serializer,
            IMemoryCache cache
            )
        {
            _settings = settings;
            _serializer = serializer;
            _cache = cache;
        }

        GangStoreFactory< TData> IGangStoreFactory
            .For<TData>()
        {
            return new GangStoreFactory<TData>(
                (name, indexers) =>new FileSystemGangStore<TData>(
                    _settings,
                    _serializer,
                    _cache,
                    name,
                    indexers
                    )
                );
        }
    }
}