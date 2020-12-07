using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gang.Authentication.Api
{
    public sealed class GangAuthenticationCredential
    {
        public GangAuthenticationCredential(
            string id,
            IEnumerable<string> transports
        )
        {
            Id = id;
            Transports = transports
                ?.ToImmutableList();
        }

        public string Id { get; }
        public IImmutableList<string> Transports { get; }
    }
}
