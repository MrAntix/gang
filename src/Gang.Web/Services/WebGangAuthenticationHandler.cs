using Gang.Contracts;
using Gang.Management;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gang.Web.Services
{
    public class WebGangAuthenticationHandler :
        IGangAuthenticationHandler
    {
        const int MAX_USERS = 10;
        readonly IGangManager _manager;
        readonly IDictionary<string, string> _memberTokenMap;

        public WebGangAuthenticationHandler(
            IGangManager manager)
        {
            _manager = manager;
            _memberTokenMap = new Dictionary<string, string>();
        }

        Task<GangAuth> IGangAuthenticationHandler.AuthenticateAsync(
           GangParameters parameters)
        {
            var gang = _manager.GangById(parameters.GangId);
            if (parameters.GangId == "demo"
                && (gang?.Members.Count ?? 0) < MAX_USERS)
            {
                // find and remove member id
                if (!_memberTokenMap.Remove(
                    parameters.Token, out var memberId))
                    memberId = Guid.NewGuid().ToString("N");

                // return and store new token for next time
                var token = Guid.NewGuid().ToString("N");
                _memberTokenMap.Add(token, memberId);

                return Task.FromResult(
                    new GangAuth(
                        Encoding.UTF8.GetBytes(memberId),
                        Encoding.UTF8.GetBytes(token)
                        )
                    );
            }

            return Task.FromResult(default(GangAuth));
        }
    }
}
