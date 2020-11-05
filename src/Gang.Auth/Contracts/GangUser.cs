using Gang.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gang.Auth.Contracts
{
    public class GangUser :
        IHasGangIdString
    {
        public GangUser(
            string id,
            string name, string emailAddress,
            IEnumerable<string> roles = null,
            string secret = null,
            GangUserToken linkToken = null
            )
        {
            Id = id;
            Name = name;
            EmailAddress = emailAddress;
            Roles = roles?.ToImmutableSortedSet()
                ?? ImmutableSortedSet<string>.Empty;
            Secret = secret ?? $"{Guid.NewGuid():N}";
            LinkToken = linkToken;
        }

        public string Id { get; }
        public string Name { get; }
        public string EmailAddress { get; }
        public IImmutableSet<string> Roles { get; }

        public string Secret { get; }
        public GangUserToken LinkToken { get; }

        public GangAuth GetAuth(string token)
        {
            return new GangAuth(
                Id,
                Name, EmailAddress,
                Roles,
                token
                );
        }

        public GangTokenData GetTokenData(DateTimeOffset expires)
        {
            return new GangTokenData(
                Id, expires,
                Name, EmailAddress,
                Roles
                );
        }

        public GangUserLink GetLink(GangUserToken token)
        {
            if (token.Value == null
                || token.Expires == null
                || token.Expires < DateTimeOffset.Now)
                throw new InvalidOperationException("Link Token Invalid");

            return new GangUserLink(
                Name, EmailAddress,
                token
                );
        }

        public GangUser SetLinkToken(GangUserToken linkToken)
        {
            return new GangUser(
                Id,
                Name, EmailAddress,
                Roles,
                Secret,
                linkToken
                );
        }

        public GangUser AddRole(string role)
        {
            return new GangUser(
                Id,
                Name, EmailAddress,
                Roles.Add(role),
                Secret,
                LinkToken
                );
        }

        public GangUser RemoveRole(string role)
        {
            return new GangUser(
                Id,
                Name, EmailAddress,
                Roles.Remove(role),
                Secret,
                LinkToken
                );
        }
    }

    public sealed class GangUserToken
    {
        public GangUserToken(
            string value,
            DateTimeOffset? expiry)
        {
            Value = value;
            Expires = expiry;
        }

        public string Value { get; }
        public DateTimeOffset? Expires { get; }
    }
}
