using Gang.Authentication.Api;
using Gang.Authentication.Tokens;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gang.Authentication.Users
{
    public sealed class GangUserData :
        IHasGangIdString
    {
        public GangUserData(
            string id,
            string name = "(new user)", string email = null,
            IEnumerable<string> roles = null,
            string secret = null,
            GangUserLinkCode linkCode = null, string challenge = null,
            IEnumerable<GangUserCredential> credentials = null
            )
        {
            Id = id;
            Name = name;
            Email = email;
            Roles = roles?.ToImmutableSortedSet()
                ?? ImmutableSortedSet<string>.Empty;
            Secret = secret ?? $"{Guid.NewGuid():N}";
            LinkCode = linkCode;
            Challenge = challenge;
            Credentials = credentials.ToImmutableListDefaultEmpty();
        }

        public string Id { get; }
        public string Name { get; }
        public string Email { get; }
        public IImmutableSet<string> Roles { get; }

        public string Secret { get; }
        public GangUserLinkCode LinkCode { get; }
        public string Challenge { get; }
        public IImmutableList<GangUserCredential> Credentials { get; }

        public GangSession GetSession(string token)
        {
            return new GangSession(
                new GangSessionUser(
                    Id,
                    Name, Email
                ),
                Roles,
                token
                );
        }

        public GangTokenData GetTokenData(DateTimeOffset expires)
        {
            return new GangTokenData(
                Id, expires,
                Name, Email,
                Roles
                );
        }

        public GangUserLink GetLink(GangUserLinkCode code, object data = null)
        {
            if (code.Value == null
                || code.Expires == null
                || code.Expires < DateTimeOffset.Now)
                throw new InvalidOperationException("Link code is invalid");

            return new GangUserLink(
                Name, Email,
                code, data
                );
        }

        public GangUserData SetName(
            string value)
        {
            return new GangUserData(
                Id,
                value, Email,
                Roles,
                Secret,
                LinkCode, Challenge,
                Credentials
                );
        }

        public GangUserData SetEmailAddress(
            string value)
        {
            return new GangUserData(
                Id,
                Name, value,
                Roles,
                Secret,
                LinkCode, Challenge,
                Credentials
                );
        }

        public GangUserData SetLinkCode(
            GangUserLinkCode value)
        {
            return new GangUserData(
                Id,
                Name, Email,
                Roles,
                Secret,
                value, Challenge,
                Credentials
                );
        }

        public GangUserData SetChallenge(
            string value)
        {
            return new GangUserData(
                Id,
                Name, Email,
                Roles,
                Secret,
                LinkCode, value,
                Credentials
                );
        }

        public GangUserData AddRole(string value)
        {
            return new GangUserData(
                Id,
                Name, Email,
                Roles.Add(value),
                Secret,
                LinkCode, Challenge,
                Credentials
                );
        }

        public GangUserData RemoveRole(string value)
        {
            return new GangUserData(
                Id,
                Name, Email,
                Roles.Remove(value),
                Secret,
                LinkCode, Challenge,
                Credentials
                );
        }

        public GangUserData SetCredential(
            GangUserCredential value)
        {
            return new GangUserData(
                Id,
                Name, Email,
                Roles,
                Secret,
                LinkCode, Challenge,
                Credentials
                    .RemoveAll(c => c.Id == value.Id)
                    .Add(value)
                );
        }

        public GangUserData RemoveExpiredCredentials(
            int expiryDays
            )
        {
            return new GangUserData(
                Id,
                Name, Email,
                Roles,
                Secret,
                LinkCode, Challenge,
                Credentials
                    .RemoveAll(c => c.Validated.AddDays(expiryDays) < DateTimeOffset.Now)
                );
        }
    }
}
