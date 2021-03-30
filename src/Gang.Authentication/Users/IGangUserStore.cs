using System.Threading.Tasks;

namespace Gang.Authentication.Users
{
    public interface IGangUserStore
    {
        Task<GangUserData> TryGetByIdAsync(string id);
        Task<GangUserData> TryGetByCredentialIdAsync(string credentialId);
        Task<GangUserData> TryGetByEmailAddressAsync(string email);
        Task<GangUserData> SetAsync(GangUserData value);
    }
}
