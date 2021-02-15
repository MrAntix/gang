using Gang.Serialization;
using Gang.Tasks;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Gang.Storage
{
    /// <summary>
    /// Store <see cref="TData"/> in the file system
    /// </summary>
    /// <typeparam name="TData">Type of data</typeparam>
    public sealed class FileSystemGangStore<TData> :
        IGangStore<TData>
    {
        readonly IFileSystemGangStoreSettings _settings;
        readonly IGangSerializationService _serializer;
        readonly IMemoryCache _cache;
        readonly TaskQueue _ioTasks;

        readonly ImmutableArray<Func<TData, IEnumerable<object>>> _indexers;

        public FileSystemGangStore(
            IFileSystemGangStoreSettings settings,
            IGangSerializationService serializer,
            IMemoryCache cache,
            string name = null,
            IEnumerable<Func<TData, IEnumerable<object>>> indexers = null
            )
        {
            _settings = settings;
            _serializer = serializer;
            _cache = cache;
            _ioTasks = new TaskQueue();

            _indexers = indexers
                ?.ToImmutableArray()
                ?? ImmutableArray<Func<TData, IEnumerable<object>>>.Empty;

            Name = name;
            Directory.CreateDirectory(Path.GetDirectoryName(GetDataFilePath(null)));
            Directory.CreateDirectory(Path.GetDirectoryName(GetIndexFilePath(null)));
        }

        public string Name { get; }

        async Task<IImmutableList<string>> IGangStore<TData>
            .TryGetIndexedKeys(object value, int skip, int? take)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (skip < 0)
                throw new ArgumentOutOfRangeException(nameof(skip));
            if (take != null && take < skip)
                throw new ArgumentOutOfRangeException(nameof(take));

            var index = await GetIndexAsync(value) as IEnumerable<string>;
            if (skip > 0) index = index.Skip(skip);
            if (take != null) index = index.Take(take.Value);

            return index.ToImmutableArray();
        }

        async Task IGangStore<TData>
            .PutAsync(string key, TData data)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            await RemoveIndexedValues(key);

            await SaveDataAsync(key, data);

            await AddIndexedValues(key, data);
        }

        async Task<bool> IGangStore<TData>
            .TryDeleteAsync(string key)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            var dataFilePath = GetDataFilePath(key);
            if (File.Exists(dataFilePath))
            {
                await RemoveIndexedValues(key);

                await DeleteFileAsync(dataFilePath);

                return true;
            }

            return false;
        }

        Task<TData> IGangStore<TData>
            .TryGetAsync(string key)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            return LoadDataAsync(key);
        }

        async Task<ImmutableArray<string>> GetIndexAsync(
            object key
            )
        {
            if (_cache.TryGetValue(key, out ImmutableArray<string> index))
                return index;

            var loadedIndex = await LoadIndexAsync(key);

            return loadedIndex != null
                ? loadedIndex
                : ImmutableArray<string>.Empty;
        }

        async Task<TData> LoadDataAsync(
            string key
            )
        {
            var filePath = GetDataFilePath(key);

            return await LoadFileAsync<TData>(filePath);
        }

        Task SaveDataAsync(
            string key,
            TData data
            )
        {
            var filePath = GetDataFilePath(key);

            return SaveFileAsync(filePath, data);
        }

        async Task<ImmutableArray<string>> LoadIndexAsync(
            object key
            )
        {
            var filePath = GetIndexFilePath(key);

            return await LoadFileAsync<ImmutableArray<string>>(filePath);
        }

        Task SaveIndexAsync(
            object key,
            IEnumerable<string> index
            )
        {
            var filePath = GetIndexFilePath(key);

            if (!index.Any())
                return DeleteFileAsync(filePath);

            return SaveFileAsync(filePath, index);
        }

        async Task AddIndexedValues(
            string key, TData data
            )
        {
            var toSave = new Dictionary<object, ImmutableArray<string>>();
            foreach (var value in _indexers.SelectMany(i => i(data)))
            {
                var index = await GetIndexAsync(value);

                if (!index.Contains(key))
                {
                    index = index.Add(key);

                    _cache.Set(value, index);
                    toSave[value] = index;
                }
            }

            foreach (var kv in toSave)
                await SaveIndexAsync(kv.Key, kv.Value);
        }

        async Task RemoveIndexedValues(string key)
        {
            var oldData = await LoadDataAsync(key);
            if (oldData == null) return;

            var toSave = new Dictionary<object, ImmutableArray<string>>();
            foreach (var oldValue in _indexers.SelectMany(i => i(oldData)))
            {
                var index = await GetIndexAsync(oldValue);

                if (index.Contains(key))
                {
                    index = index.Remove(key);

                    _cache.Set(oldValue, index);
                    toSave[oldValue] = index;
                }
            }

            foreach (var kv in toSave)
                await SaveIndexAsync(kv.Key, kv.Value);
        }

        Task SaveFileAsync(string filePath, object content)
        {
            return _ioTasks.Enqueue(() =>
            {
                return File.WriteAllBytesAsync(
                    filePath,
                    _serializer.Serialize(content)
                    );
            });
        }

        Task DeleteFileAsync(string filePath)
        {
            return _ioTasks.Enqueue(() =>
            {
                File.Delete(filePath);
                return Task.CompletedTask;
            });
        }

        async Task<TContent> LoadFileAsync<TContent>(string filePath)
        {
            if (!File.Exists(filePath)) return default;

            var data = await File.ReadAllBytesAsync(filePath);

            return _serializer.Deserialize<TContent>(data);
        }

        string GetDataFilePath(object key)
        {
            return $"{_settings.RootPath}/{Name}/{key}{_settings.FileExtension}";
        }

        string GetIndexFilePath(object key)
        {
            return $"{_settings.RootPath}/{Name}/.index/{key}{_settings.FileExtension}";
        }
    }
}
