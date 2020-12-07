namespace Gang.Storage
{
    public sealed class InMemoryGangStoreFactory :
        IGangStoreFactory
    {
        GangStoreFactory<TData> IGangStoreFactory.For<TData>()
        {
            return new GangStoreFactory<TData>(
                (_, indexers) => new InMemoryGangStore<TData>(
                    indexers
                    )
                );
        }
    }
}
