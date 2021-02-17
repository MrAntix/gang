using Gang.Storage;
using System.Threading.Tasks;
using Xunit;

namespace Gang.Tests.Storage
{
    public class InMemoryStoreTests
    {
        static IGangStoreFactory GetServiceFactory()
        {
            return new InMemoryGangStoreFactory();
        }

        sealed record A
        {
            public string Prop { get; init; }
        }

        [Fact]
        public async Task put_get()
        {
            var service = GetServiceFactory()
                .For<A>()
                .Create();

            var k = "Key";

            var a = new A { Prop = "VALUE" };
            await service.SetAsync(k, a);

            var got = await service.TryGetAsync(k);

            Assert.Equal(a, got);
        }

        [Fact]
        public async Task put_with_index()
        {
            var service = GetServiceFactory()
                .For<A>()
                .AddIndex(o => new[] { o.Prop })
                .Create();

            var k = "Key";

            var a = new A { Prop = "VALUE" };
            await service.SetAsync(k, a);

            Assert.Equal(k, Assert.Single(
                await service.TryGetIndexedKeys(a.Prop)
                ));
        }

        [Fact]
        public async Task put_with_index_then_update_indexed_value()
        {
            var service = GetServiceFactory()
                .For<A>()
                .AddIndex(o => o.Prop)
                .Create();

            var k = "Key";

            var a = new A { Prop = "VALUE" };
            await service.SetAsync(k, a);

            var update = a with { Prop = "VALUE_UPDATED" };
            await service.SetAsync(k, update);

            Assert.Empty(await service.TryGetIndexedKeys(a.Prop));

            Assert.Equal(k, Assert.Single(
                await service.TryGetIndexedKeys(update.Prop)
                ));
        }

        [Fact]
        public async Task put_with_index_then_delete()
        {
            var service = GetServiceFactory()
                .For<A>()
                .AddIndex(o => o.Prop)
                .Create();

            var k = "Key";

            var a = new A { Prop = "VALUE" };
            await service.SetAsync(k, a);

            Assert.True(await service.TryDeleteAsync(k));

            Assert.Null(await service.TryGetAsync(k));
            Assert.Empty(await service.TryGetIndexedKeys(a.Prop));
        }

        [Fact]
        public async Task put_multiple_with_index_same_values()
        {
            var service = GetServiceFactory()
                .For<A>()
                .AddIndex(o => o.Prop)
                .Create();

            var k1 = "Key1";
            var a1 = new A { Prop = "VALUE" };
            await service.SetAsync(k1, a1);

            var k2 = "Key2";
            var a2 = new A { Prop = "VALUE" };
            await service.SetAsync(k2, a2);

            Assert.Equal(new[] { k1, k2 },
                await service.TryGetIndexedKeys(a1.Prop)
                );
        }
    }
}
