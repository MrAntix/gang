using Gang.Contracts;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Gang.Web.Services
{
    public class WebGangAuthenticationHandler :
        IGangAuthenticationHandler
    {
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
                && (gang?.Members.Count ?? 0) < 4)
            {
                return Task.FromResult(
                    Encoding.UTF8.GetBytes($"{Guid.NewGuid():N}"));
            }

            return Task.FromResult(default(byte[]));
        }
    }
}
