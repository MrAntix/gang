using Gang.Authentication.Api;
using System.Threading.Tasks;

namespace Gang.Authentication
{
    public interface IGangAuthenticationService
    {
        Task RequestLinkAsync(string email, object data = null);
        Task<string> ValidateLinkAsync(GangLink data);

        Task<string> RequestChallengeAsync(string token);
        Task<GangSession> AuthenticateAsync(string token);
        Task<bool> RegisterCredentialAsync(string token, GangCredentialRegistration data);
        Task<string> ValidateCredentialAsync(GangAuthentication data);
    }
}
