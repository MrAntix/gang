using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gang.Authentication.Tokens
{
    public sealed class GangTokenData
    {
        public GangTokenData(
            string id, DateTimeOffset expires,
            string name = null, string emailAddress = null,
            IEnumerable<string> roles = null)
        {
            Id = id;
            Expires = expires;
            Name = name;
            EmailAddress = emailAddress;
            Roles = roles?.ToImmutableSortedSet()
               ?? ImmutableSortedSet<string>.Empty;
        }

        public string Id { get; }
        public DateTimeOffset Expires { get; }

        public string Name { get; }
        public string EmailAddress { get; }
        public IImmutableSet<string> Roles { get; }
    }
}
