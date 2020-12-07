using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gang.Storage
{
    public sealed class GangStoreFactory< TData>
    {
        readonly Func<string, IEnumerable<Func<TData, IEnumerable<object>>>, IGangStore<TData>> _create;
        readonly ImmutableArray<Func<TData, IEnumerable<object>>> _indexers;

        public GangStoreFactory(
            Func<string, IEnumerable<Func<TData, IEnumerable<object>>>, IGangStore<TData>> create,
            IEnumerable<Func<TData, IEnumerable<object>>> indexers = null
            )
        {
            _create = create;
            _indexers = indexers
                ?.ToImmutableArray()
                ?? ImmutableArray<Func<TData, IEnumerable<object>>>.Empty;
        }

        public GangStoreFactory< TData> AddIndex(
            Func<TData, IEnumerable<object>> indexer)
        {
            if (indexer is null)
                throw new ArgumentNullException(nameof(indexer));

            return new GangStoreFactory< TData>(
                _create,
                _indexers.Add(indexer)
                );
        }

        public IGangStore<TData> Create(string name = null)
        {
            return _create(name, _indexers);
        }
    }
}
