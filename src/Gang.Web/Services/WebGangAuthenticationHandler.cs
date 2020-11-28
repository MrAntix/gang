using Gang.Authentication;
using Gang.Management;
using System;
using System.Threading.Tasks;

namespace Gang.Web.Services
{
    public sealed class WebGangAuthenticationHandler :
        IGangAuthenticationHandler
    {
        const int MAX_USERS = 10;
        readonly IGangManager _manager;

        public WebGangAuthenticationHandler(
            IGangManager manager)
        {
            _manager = manager;
        }

        Task<GangAuth> IGangAuthenticationHandler.AuthenticateAsync(
           GangParameters parameters)
        {
            var gang = _manager.GangById(parameters.GangId);
            var auth = default(GangAuth);
            if (parameters.GangId == "demo"
                && (gang?.Members.Count ?? 0) < MAX_USERS)
            {
                var id = parameters.Token ?? $"{Guid.NewGuid():N}";

                auth = new GangAuth(
                    id,
                    token: id
                    );
            }

            return Task.FromResult(auth);
        }
    }
}
