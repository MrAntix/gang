using Gang.Authentication.Api;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gang.Authentication.Users
{
    public sealed class GangUserCredential
    {
        public GangUserCredential(
            string id,
            GangPublicKey publicKey,
            IEnumerable<string> transports,
            DateTimeOffset created
            )
        {
            Id = id;
            PublicKey = publicKey;
            Transports = transports?.ToImmutableList();
            Created = created;
        }

        public string Id { get; }
        public GangPublicKey PublicKey { get; }
        public IImmutableList<string> Transports { get; }
        public DateTimeOffset Created { get; }

        public static GangUserCredential Create(
            GangCredentialRegistration data)
        {
            return new GangUserCredential(
                data.CredentialId,
                data.PublicKey,
                data.Transports,
                DateTimeOffset.UtcNow
                );
        }
    }
}
