using Gang.Storage;
using System.Linq;
using System.Threading.Tasks;

namespace Gang.Authentication.Users
{
    public sealed class GangUserStore :
        IGangUserStore
    {
        readonly IGangStore<GangUserData> _store;

        public GangUserStore(
            IGangStoreFactory storeFactory
            )
        {
            _store = storeFactory
                .For<GangUserData>()
                .AddIndex(d => d.Email)
                .AddIndex(d => d.Credentials.Select(c => c.Id))
                .Create("users");
        }

        Task<GangUserData> IGangUserStore
            .SetAsync(GangUserData value)
        {
            return _store.SetAsync(value.Id, value);
        }

        async Task<GangUserData> IGangUserStore
            .TryGetByCredentialIdAsync(string credentialId)
        {
            var id = await _store.TryGetIndexedKeyAsync(credentialId);
            if (id == null) return null;

            return await _store.TryGetAsync(id);
        }

        async Task<GangUserData> IGangUserStore
            .TryGetByEmailAddressAsync(string email)
        {
            var id = await _store.TryGetIndexedKeyAsync(email);
            if (id == null) return null;

            return await _store.TryGetAsync(id);
        }

        async Task<GangUserData> IGangUserStore
            .TryGetByIdAsync(string id)
        {
            return await _store.TryGetAsync(id);
        }
    }
}
