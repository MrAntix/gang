using Gang.Auth.Contracts;
using System.Threading.Tasks;

namespace Gang.Auth
{
    public interface IGangAuthUserStore
    {
        Task<GangUser> TryGetAsync(string id);
        Task<GangUser> TryGetByEmailAddressAsync(string emailAddress);
        Task<GangUser> TryGetByLinkTokenAsync(string token);
        Task SetAsync(GangUser value);
    }
}
