using Gang.Contracts;
using System.Text;
using System.Threading.Tasks;

namespace Gang.Web.Services
{
    public class WebGangAuthenticationHandler :
        IGangAuthenticationHandler
    {
        const int MAX_USERS = 10;
        readonly IGangHandler _handler;

        public WebGangAuthenticationHandler(
            IGangHandler handler)
        {
            _handler = handler;
        }

        Task<byte[]> IGangAuthenticationHandler.AuthenticateAsync(
           GangParameters parameters)
        {
            var gang = _handler.GangById(parameters.GangId);
            if (parameters.GangId == "demo"
                && parameters.Token?.Length == 32
                && (gang?.Members.Count ?? 0) < MAX_USERS)
            {
                return Task.FromResult(
                    Encoding.UTF8.GetBytes(parameters.Token));
            }

            return Task.FromResult(default(byte[]));
        }
    }
}
