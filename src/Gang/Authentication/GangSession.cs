using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gang.Authentication
{
    public sealed class GangSession
    {
        public GangSession(
            GangSessionUser user,
            IEnumerable<string> roles,
            string token)
        {
            User = user ?? throw new System.ArgumentNullException(nameof(user));
            Roles = roles?.ToImmutableSortedSet()
               ?? ImmutableSortedSet<string>.Empty;
            Token = token;
        }

        public GangSession(
            string userId,
            string token) :
            this(new GangSessionUser(userId), null, token)
        {
        }

        public GangSessionUser User { get; }
        public IImmutableSet<string> Roles { get; }
        public string Token { get; }

        public static GangSession Default { get; } = new GangSession(null, null);
    }
}
