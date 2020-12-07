using Gang.Authentication.Tokens;
using Gang.Authentication.Users;
using Gang.Management;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Gang.Authentication
{
    public sealed class GangAuthenticationService :
        IGangAuthenticationService
    {
        public const int DEFAULT_LINK_EXPIRY_MINUTES = 15;
        public const int DEFAULT_SESSION_EXPIRY_MINUTES = 60 * 24 * 7;

        private readonly ILogger<GangAuthenticationService> _logger;
        private readonly IGangAuthenticationSettings _settings;
        readonly IGangTokenService _tokens;
        readonly IGangManager _manager;
        readonly IGangAuthenticationUserStore _users;
        readonly IGangLinkService _linkServices;

        public GangAuthenticationService(
            ILogger<GangAuthenticationService> logger,
            IGangAuthenticationSettings settings,
            IGangTokenService tokens,
            IGangManager manager,
            IGangAuthenticationUserStore users,
            IGangLinkService linkServices)
        {
            _logger = logger;
            _settings = settings;
            _tokens = tokens;
            _manager = manager;
            _users = users;
            _linkServices = linkServices;
        }

        async Task IGangAuthenticationService
            .RequestLinkAsync(string emailAddress, object data)
        {
            _logger.LogDebug($"Requesting link for {emailAddress}");

            var user = await _users.TryGetByEmailAddressAsync(emailAddress);
            if (user == null)
            {
                user = new GangUserData(
                    $"{Guid.NewGuid():N}",
                    null, emailAddress
                    );
            }

            var token = new GangUserLinkCode(
                _linkServices.CreateCode(),
                DateTimeOffset.Now.AddMinutes(_settings.LinkExpiryMinutes ?? DEFAULT_LINK_EXPIRY_MINUTES)
                );
            user = user.SetLinkCode(token);

            await _users.SetAsync(user);

            _manager.RaiseEvent(
                user.GetLink(token, data)
                );
        }

        async Task<string> IGangAuthenticationService
            .LinkAsync(string token)
        {
            var user = await _users.TryGetByLinkTokenAsync(token);
            _logger.LogDebug($"Link token {token} => user {user?.Id}");

            if (user?.LinkCode == null) return null;

            var expires = user.LinkCode.Expires;

            // clear the link token
            user = user.SetLinkCode(null);
            await _users.SetAsync(user);

            if (expires < DateTimeOffset.Now) return null;

            return GetToken(user);
        }

        async Task<GangSession> IGangAuthenticationService
            .AuthenticateAsync(string token)
        {
            var auth = _tokens.TryDecode(token);
            if (auth == null
                || auth.Expires < DateTimeOffset.Now) return null;

            var user = await _users.TryGetByIdAsync(auth.Id);
            //var user = await _users.TryGetByEmailAddressAsync(auth.EmailAddress);
            if (user == null) return null;

            if (!_tokens.Verify(token, user.Secret)) return null;

            var newToken = GetToken(user);

            return user.GetAuth(newToken);
        }

        string GetToken(GangUserData user)
        {
            var tokenData = user.GetTokenData(
                DateTimeOffset.Now.AddMinutes(_settings.SessionExpiryMinutes ?? DEFAULT_SESSION_EXPIRY_MINUTES)
                );
            return _tokens.Create(tokenData, user.Secret);
        }
    }
}
