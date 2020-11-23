using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gang.Authentication
{
    public class GangAuth
    {
        public GangAuth(
            string id,
            string name = null, string emailAddress = null,
            IEnumerable<string> roles = null,
            string token = null)
        {
            Id = id;
            Name = name;
            EmailAddress = emailAddress;
            Roles = roles?.ToImmutableSortedSet()
               ?? ImmutableSortedSet<string>.Empty;
            Token = token;
        }

        public string Id { get; }
        public string Name { get; }
        public string EmailAddress { get; }
        public IImmutableSet<string> Roles { get; }

        public string Token { get; }
    }
}
