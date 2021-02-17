namespace Gang.Storage
{
    public sealed class InMemoryGangStoreFactory :
        IGangStoreFactory
    {
        GangStoreFactory<TData> IGangStoreFactory.For<TData>()
        {
            return new GangStoreFactory<TData>(
                (name, indexers) => new InMemoryGangStore<TData>(
                    name,
                    indexers
                    )
                );
        }
    }
}
