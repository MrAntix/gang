using System.Threading.Tasks;

namespace Gang.Authentication
{
    public interface IGangAuthenticationService
    {
        Task RequestLink(string emailAddress, object data = null);
        Task<string> Link(string token);
        Task<GangAuth> AuthenticateAsync(string token);
    }
}
