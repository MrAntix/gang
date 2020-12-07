using System.Threading.Tasks;

namespace Gang.Authentication
{
    public interface IGangAuthenticationHandler
    {
        Task<GangSession> HandleAsync(GangParameters parameters);
    }
}
