using Gang.Storage;
using System.Threading.Tasks;

namespace Gang.Authentication
{
    public sealed class GangSessionUserStore :
        IGangSessionUserStore
    {
        readonly IGangStore<GangSessionUser> _store;

        public GangSessionUserStore(
            IGangStoreFactory storeFactory
            )
        {
            _store = storeFactory
                .For<GangSessionUser>()
                .Create("users");
        }

        Task IGangSessionUserStore
            .SetAsync(GangSessionUser value)
        {
            return _store.SetAsync(value.Id, value);
        }

        async Task<GangSessionUser> IGangSessionUserStore
            .TryGetByIdAsync(string id)
        {
            return await _store.TryGetAsync(id);
        }
    }
}
