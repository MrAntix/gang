
using Gang.Authentication.Api;
using Gang.Authentication.Crypto;
using Gang.Authentication.Tokens;
using Gang.Authentication.Users;
using Gang.Management;
using Gang.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Gang.Authentication
{
    public sealed class GangAuthenticationService :
        IGangAuthenticationService
    {
        public const int DEFAULT_LINK_PARTS = 2;
        public const int DEFAULT_LINK_EXPIRY_MINUTES = 15;
        public const int DEFAULT_SESSION_EXPIRY_MINUTES = 60 * 24 * 7;

        readonly ILogger<GangAuthenticationService> _logger;
        readonly IGangAuthenticationSettings _settings;
        readonly IGangTokenService _tokens;
        readonly IGangManager _manager;
        readonly IGangUserStore _users;
        readonly IGangCryptoService _crypto;

        public GangAuthenticationService(
            ILogger<GangAuthenticationService> logger,
            IGangAuthenticationSettings settings,
            IGangTokenService tokens,
            IGangManager manager,
            IGangUserStore users,
            IGangCryptoService crypto)
        {
            _logger = logger;
            _settings = settings;
            _tokens = tokens;
            _manager = manager;
            _users = users;
            _crypto = crypto;
        }

        async Task IGangAuthenticationService
            .RequestLinkAsync(string email, object data)
        {
            _logger.LogDebug($"Requesting link for {email}");

            var user = await _users.TryGetByEmailAddressAsync(email);
            if (user == null)
            {
                user = new GangUserData(
                    $"{Guid.NewGuid():N}",
                    null, email
                    );
            }

            var linkCode = new GangUserLinkCode(
                _crypto.GetRandom(_settings.LinkParts ?? DEFAULT_LINK_PARTS),
                DateTimeOffset.Now.AddMinutes(_settings.LinkExpiryMinutes ?? DEFAULT_LINK_EXPIRY_MINUTES)
                );
            user = user.SetLinkCode(linkCode);

            await _users.SetAsync(user);

            _manager.RaiseEvent(
                user.GetLink(linkCode, data)
                );
        }

        async Task<string> IGangAuthenticationService
            .ValidateLinkAsync(GangLink data)
        {
            if (string.IsNullOrWhiteSpace(data?.Email))
                throw new ArgumentException($"'{nameof(data.Email)}' cannot be null or whitespace.", nameof(data.Email));
            if (string.IsNullOrWhiteSpace(data?.Code))
                throw new ArgumentException($"'{nameof(data.Code)}' cannot be null or whitespace.", nameof(data.Code));

            var user = await _users.TryGetByEmailAddressAsync(data.Email);
            if (user == null) return null;

            if (user.LinkCode?.Value != data.Code) return null;

            var expires = user.LinkCode.Expires;

            // clear the link token
            user = user.SetLinkCode(null);
            await _users.SetAsync(user);

            if (expires < DateTimeOffset.Now) return null;

            _logger.LogDebug($"Link code {data.Code} => user {user.Id}");

            return GetToken(user);
        }

        async Task<GangSession> IGangAuthenticationService
            .AuthenticateAsync(string token)
        {
            var user = await GetValidUserAsync(token);
            if (user == null) return null;

            return user.GetSession(GetToken(user));
        }

        async Task<string> IGangAuthenticationService
            .RequestChallengeAsync(string token)
        {
            var user = await GetValidUserAsync(token);
            if (user == null) return null;

            user = user.SetChallenge($"{Guid.NewGuid():N}");
            await _users.SetAsync(user);

            return user.Challenge;
        }

        async Task<bool> IGangAuthenticationService
            .RegisterCredentialAsync(
                string token,
                GangCredentialRegistration data
            )
        {
            var user = await GetValidUserAsync(token);
            if (user == null) return false;

            if (user.Challenge != data.Challenge) return false;

            user = user.SetCredential(
                GangUserCredential.Create(data)
                );
            await _users.SetAsync(user);

            return true;
        }

        async Task<string> IGangAuthenticationService
            .ValidateCredentialAsync(
                GangAuthentication data
            )
        {
            var user = await _users.TryGetByCredentialIdAsync(data.CredentialId);
            if (user == null) return null;

            var credential = user.Credentials.First(c => c.Id == data.CredentialId);

            if (!ValidateCredential(
                credential.PublicKey,
                data.ClientData, data.AuthenticatorData, data.Signature))
                return null;

            user = user
                .RemoveExpiredCredentials(_settings.CredentialExpiryDays ?? 60)
                .SetCredential(
                    credential.SetValidated()
                );
            await _users.SetAsync(user);

            return GetToken(user);
        }

        bool ValidateCredential(
            GangPublicKey publicKey,
            string clientData,
            string authenticatorData,
            string signatureData
            )
        {
            var authenticatorBytes = GangSerialization
                .Base64UrlToBytes(authenticatorData);

            // RP ID Hash
            //var rpIdHash = authenticatorBytes[..32];

            // Flags
            //var flags = new BitArray(authenticatorBytes[32..33].ToArray());
            //var userPresent = flags[0];
            //var userVerified = flags[2];
            //var attestedCredentialData = flags[6];
            //var extensionDataIncluded = flags[7];

            // Signature counter
            //var counterBuf = authenticatorBytes[33..37].ToArray();
            //var counter = BitConverter.ToUInt32(counterBuf);

            var signature = GangSerialization.Base64UrlToBytes(signatureData);

            using var hasher = new SHA256Managed();

            var hash = hasher.ComputeHash(
                 GangSerialization.Base64UrlToBytes(clientData).ToArray()
                );

            var data = new byte[authenticatorBytes.Length + hash.Length];
            authenticatorBytes.CopyTo(data);
            hash.CopyTo(data, authenticatorBytes.Length);

            return _crypto.VerifySignature(
                publicKey,
                data, signature
                );
        }

        async Task<GangUserData> GetValidUserAsync(string token)
        {
            var auth = _tokens.TryDecode(token);
            if (auth == null
                || auth.Expires < DateTimeOffset.Now) return null;

            var user = await _users.TryGetByIdAsync(auth.Id);
            if (user == null) return null;

            if (!_tokens.Verify(token, user.Secret)) return null;

            return user;
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
