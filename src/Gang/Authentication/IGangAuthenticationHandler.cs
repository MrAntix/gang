using System.Threading.Tasks;

namespace Gang.Authentication
{
    public interface IGangAuthenticationHandler
    {
        Task<GangAuth> AuthenticateAsync(GangParameters parameters);
    }
}
