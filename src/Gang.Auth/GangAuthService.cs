using Gang.Auth.Contracts;
using Gang.Contracts;
using Gang.Management;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Gang.Auth
{
    public class GangAuthService :
        IGangAuthService
    {
        private readonly ILogger<GangAuthService> _logger;
        private readonly GangAuthSettings _settings;
        readonly IGangTokenService _tokens;
        readonly IGangManager _manager;
        readonly IGangAuthUserStore _users;

        public GangAuthService(
            ILogger<GangAuthService> logger,
            GangAuthSettings settings,
            IGangTokenService tokens,
            IGangManager manager,
            IGangAuthUserStore users)
        {
            _logger = logger;
            _settings = settings;
            _tokens = tokens;
            _manager = manager;
            _users = users;
        }

        async Task IGangAuthService
            .RequestLink(string emailAddress)
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
                user.GetLink(token)
                );
        }

        async Task<string> IGangAuthService
            .Link(string token)
        {
            var user = await _users.TryGetByLinkTokenAsync(token);
            _logger.LogDebug($"Link token {token} => user {user?.Id}");

            if (user == null
                || user.LinkToken.Expires < DateTimeOffset.Now) return null;

            return GetToken(user);
        }

        async Task<GangAuth> IGangAuthService
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
