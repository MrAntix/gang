using Gang.Contracts;
using System.Threading.Tasks;

namespace Gang.Management
{
    public interface IGangAuthenticationHandler
    {
        Task<byte[]> AuthenticateAsync(GangParameters parameters);
    }
}
