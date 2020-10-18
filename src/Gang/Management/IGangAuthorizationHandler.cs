using Gang.Contracts;
using System.Threading.Tasks;

namespace Gang.Management
{
    public interface IGangAuthenticationHandler
    {
        Task<GangAuth> AuthenticateAsync(GangParameters parameters);
    }
}
