using System.Threading.Tasks;

namespace Gang.Authentication
{
    public interface IGangAuthenticationService
    {
        Task RequestLinkAsync(string emailAddress, object data = null);
        Task<string> LinkAsync(string token);

        Task<GangSession> AuthenticateAsync(string token);
    }
}
