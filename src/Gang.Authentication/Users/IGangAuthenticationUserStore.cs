using System.Threading.Tasks;

namespace Gang.Authentication.Users
{
    public interface IGangAuthenticationUserStore
    {
        Task<GangUser> TryGetAsync(string id);
        Task<GangUser> TryGetByEmailAddressAsync(string emailAddress);
        Task<GangUser> TryGetByLinkTokenAsync(string token);
        Task SetAsync(GangUser value);
    }
}
