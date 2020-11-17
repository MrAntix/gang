using Gang.Contracts;
using System.Threading.Tasks;

namespace Gang.Auth
{
    public interface IGangAuthService
    {
        Task RequestLink(string emailAddress, object data = null);
        Task<string> Link(string token);
        Task<GangAuth> AuthenticateAsync(string token);
    }
}
