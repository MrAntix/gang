using Gang.Contracts;
using System.Threading.Tasks;

namespace Gang
{
    public interface IGangAuthenticationHandler
    {
        Task<byte[]> AuthenticateAsync(GangParameters parameters);
    }
}
