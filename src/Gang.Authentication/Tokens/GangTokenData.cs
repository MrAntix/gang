using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gang.Authentication.Tokens
{
    public sealed class GangTokenData
    {
        public GangTokenData(
            string id, DateTimeOffset expires,
            string name = null, string email = null,
            IEnumerable<string> roles = null)
        {
            Id = id;
            Expires = expires;
            Name = name;
            Email = email;
            Roles = roles?.ToImmutableSortedSet()
               ?? ImmutableSortedSet<string>.Empty;
        }

        public string Id { get; }
        public DateTimeOffset Expires { get; }

        public string Name { get; }
        public string Email { get; }
        public IImmutableSet<string> Roles { get; }
    }
}
