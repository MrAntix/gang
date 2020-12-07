using Gang.Authentication.Api;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Gang.Authentication.Users
{
    public static class GangUserMappings
    {
        public static GangAuthenticationCredential ToContract(
            this GangUserCredential source
            )
        {
            if (source == null) return null;

            return new GangAuthenticationCredential(
                source.Id,
                source.Transports
                );
        }

        public static IImmutableList<GangAuthenticationCredential> ToContract(
            this IEnumerable<GangUserCredential> source
            )
        {
            return source?.Select(ToContract).ToImmutableList();
        }
    }
}
