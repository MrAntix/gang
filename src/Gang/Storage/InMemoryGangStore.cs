using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Gang.Storage
{
    /// <summary>
    /// Store <see cref="TData"/> in memory
    /// </summary>
    /// <typeparam name="TData">Type of data</typeparam>
    public sealed class InMemoryGangStore<TData> :
        IGangStore<TData>
    {
        readonly ImmutableArray<Func<TData, IEnumerable<object>>> _indexers;

        readonly Dictionary<string, TData> _data;
        readonly Dictionary<object, ImmutableArray<string>> _indexedData;

        public InMemoryGangStore(
            string name = null,
            IEnumerable<Func<TData, IEnumerable<object>>> indexers = null
            )
        {
            Name = name;

            _indexers = indexers
                ?.ToImmutableArray()
                ?? ImmutableArray<Func<TData, IEnumerable<object>>>.Empty;

            _data = new Dictionary<string, TData>();
            _indexedData = new Dictionary<object, ImmutableArray<string>>();
        }

        public string Name { get; }

        Task<IImmutableList<string>> IGangStore<TData>
            .TryGetIndexedKeys(object value, int skip, int? take)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (skip < 0)
                throw new ArgumentOutOfRangeException(nameof(skip));
            if (take != null && take < skip)
                throw new ArgumentOutOfRangeException(nameof(take));

            IEnumerable<string> index = ImmutableArray<string>.Empty;

            if (_indexedData.ContainsKey(value))
            {
                index = _indexedData[value];
                if (skip > 0) index = index.Skip(skip);
                if (take != null) index = index.Take(take.Value);
            }

            return Task.FromResult<IImmutableList<string>>(
                index.ToImmutableArray()
                );
        }

        Task<TData> IGangStore<TData>
            .SetAsync(string key, TData data, bool overwrite)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            RemoveIndexedValues(key);

            if (!overwrite && _data.ContainsKey(key))
                throw new GangStoreException($"{key} already exists in store {Name}");

            _data[key] = data;

            AddIndexedValues(key, data);

            return Task.FromResult(data);
        }

        Task<bool> IGangStore<TData>
            .TryDeleteAsync(string key)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            if (_data.ContainsKey(key))
            {
                RemoveIndexedValues(key);
                _data.Remove(key);

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        Task<TData> IGangStore<TData>
            .TryGetAsync(string key)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            var result = default(TData);

            if (_data.ContainsKey(key))
                result = _data[key];

            return Task.FromResult(result);
        }

        void AddIndexedValues(string key, TData data)
        {
            foreach (var value in _indexers.SelectMany(i => i(data)))
            {
                if (!_indexedData.ContainsKey(value))
                    _indexedData[value] = ImmutableArray<string>.Empty;

                _indexedData[value] = _indexedData[value].Add(key);
            }
        }

        void RemoveIndexedValues(string key)
        {
            if (!_data.ContainsKey(key)) return;

            var oldData = _data[key];

            foreach (var oldValue in _indexers.SelectMany(i => i(oldData)))
            {
                _indexedData[oldValue] = _indexedData[oldValue]
                    .Where(k => k != key)
                    .ToImmutableArray();
            }
        }
    }
}
