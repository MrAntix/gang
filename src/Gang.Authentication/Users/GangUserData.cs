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
            string name = "(new user)", string emailAddress = null,
            IEnumerable<string> roles = null,
            string secret = null,
            GangUserLinkCode linkCode = null
            )
        {
            Id = id;
            Name = name;
            EmailAddress = emailAddress;
            Roles = roles?.ToImmutableSortedSet()
                ?? ImmutableSortedSet<string>.Empty;
            Secret = secret ?? $"{Guid.NewGuid():N}";
            LinkCode = linkCode;
        }

        public string Id { get; }
        public string Name { get; }
        public string EmailAddress { get; }
        public IImmutableSet<string> Roles { get; }

        public string Secret { get; }
        public GangUserLinkCode LinkCode { get; }

        public GangSession GetAuth(string token)
        {
            return new GangSession(
                new GangSessionUser(
                    Id,
                    Name, EmailAddress
                ),
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

        public GangUserLink GetLink(GangUserLinkCode code, object data = null)
        {
            if (code.Value == null
                || code.Expires == null
                || code.Expires < DateTimeOffset.Now)
                throw new InvalidOperationException("Link code is invalid");

            return new GangUserLink(
                Name, EmailAddress,
                code, data
                );
        }

        public GangUserData SetName(
            string name)
        {
            return new GangUserData(
                Id,
                name, EmailAddress,
                Roles,
                Secret,
                LinkCode
                );
        }

        public GangUserData SetEmailAddress(
            string emailAddress)
        {
            return new GangUserData(
                Id,
                Name, emailAddress,
                Roles,
                Secret,
                LinkCode
                );
        }

        public GangUserData SetLinkCode(GangUserLinkCode linkCode)
        {
            return new GangUserData(
                Id,
                Name, EmailAddress,
                Roles,
                Secret,
                linkCode
                );
        }

        public GangUserData AddRole(string role)
        {
            return new GangUserData(
                Id,
                Name, EmailAddress,
                Roles.Add(role),
                Secret,
                LinkCode
                );
        }

        public GangUserData RemoveRole(string role)
        {
            return new GangUserData(
                Id,
                Name, EmailAddress,
                Roles.Remove(role),
                Secret,
                LinkCode
                );
        }
    }
}
