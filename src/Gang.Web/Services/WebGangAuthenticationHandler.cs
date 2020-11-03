using Gang.Contracts;
using Gang.Management;
using Gang.Serialization;
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
        readonly IGangSerializationService _serializer;
        readonly IDictionary<string, string> _memberTokenMap;

        public WebGangAuthenticationHandler(
            IGangManager manager,
            IGangSerializationService serializer)
        {
            _manager = manager;
            _serializer = serializer;
            _memberTokenMap = new Dictionary<string, string>();
        }

        async Task<GangAuth> IGangAuthenticationHandler.AuthenticateAsync(
           GangParameters parameters)
        {
            var gang = _manager.GangById(parameters.GangId);
            if (parameters.GangId == "demo"
                && (gang?.Members.Count ?? 0) < MAX_USERS)
            {
                var user = await GetUserAsync(parameters.Token);

                return new GangAuth(
                        Encoding.UTF8.GetBytes(user.Id),
                        Encoding.UTF8.GetBytes(user.Token)
                        );
            }

            return default(GangAuth);
        }

        // example function which would call out to get user data based on passed token
        Task<WebGangUser> GetUserAsync(string token)
        {
            if (!_memberTokenMap.Remove(
                token, out var id))
                id = Guid.NewGuid().ToString("N");

            var properties = _serializer.Serialize(new[]
                {
                    "isUser"
                });

            // create new token
            var newToken = $"{Convert.ToBase64String(properties)}.{Guid.NewGuid():N}";

            _memberTokenMap.Add(newToken, id);

            return Task.FromResult(
                new WebGangUser(id, newToken)
            );
        }
    }
}
