using System.Threading.Tasks;

namespace Gang.Authentication.Users
{
    public interface IGangAuthenticationUserStore
    {
        Task<GangUserData> TryGetByIdAsync(string id);
        Task<GangUserData> TryGetByEmailAddressAsync(string emailAddress);
        Task<GangUserData> TryGetByLinkTokenAsync(string token);
        Task SetAsync(GangUserData value);
    }
}
