using Gang.Authentication.Tokens;
using Gang.Authentication.Users;
using Gang.Management;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Gang.Authentication
{
    public class GangAuthenticationService :
        IGangAuthenticationService
    {
        private readonly ILogger<GangAuthenticationService> _logger;
        private readonly GangAuthenticationSettings _settings;
        readonly IGangTokenService _tokens;
        readonly IGangManager _manager;
        readonly IGangAuthenticationUserStore _users;

        public GangAuthenticationService(
            ILogger<GangAuthenticationService> logger,
            GangAuthenticationSettings settings,
            IGangTokenService tokens,
            IGangManager manager,
            IGangAuthenticationUserStore users)
        {
            _logger = logger;
            _settings = settings;
            _tokens = tokens;
            _manager = manager;
            _users = users;
        }

        async Task IGangAuthenticationService
            .RequestLink(string emailAddress, object data)
        {
            _logger.LogDebug($"Requesting link for {emailAddress}");

            var user = await _users.TryGetByEmailAddressAsync(emailAddress);
            if (user == null)
            {
                user = new GangUser(
                    $"{Guid.NewGuid():N}",
                    emailAddress, emailAddress
                    );
            }

            var token = new GangUserToken(
                $"{Guid.NewGuid():N}",
                DateTimeOffset.Now.AddMinutes(_settings.LinkTokenExpiryMinutes)
                );
            user = user.SetLinkToken(token);

            await _users.SetAsync(user);

            _manager.RaiseEvent(
                user.GetLink(token, data)
                );
        }

        async Task<string> IGangAuthenticationService
            .Link(string token)
        {
            var user = await _users.TryGetByLinkTokenAsync(token);
            _logger.LogDebug($"Link token {token} => user {user?.Id}");

            if (user?.LinkToken == null) return null;

            var expires = user.LinkToken.Expires;

            // clear the link token
            user = user.SetLinkToken(null);
            await _users.SetAsync(user);

            if (expires < DateTimeOffset.Now) return null;

            return GetToken(user);
        }

        async Task<GangAuth> IGangAuthenticationService
            .AuthenticateAsync(string token)
        {
            var auth = _tokens.TryDecode(token);
            if (auth == null
                || auth.Expires < DateTimeOffset.Now) return null;

            var user = await _users.TryGetByEmailAddressAsync(auth.EmailAddress);
            if (user == null) return null;

            if (!_tokens.Verify(token, user.Secret)) return null;

            var newToken = GetToken(user);

            return user.GetAuth(newToken);
        }

        string GetToken(GangUser user)
        {
            var tokenData = user.GetTokenData(
                DateTimeOffset.Now.AddMinutes(_settings.SessionTokenExpiryMinutes)
                );
            return _tokens.Create(tokenData, user.Secret);
        }
    }
}
