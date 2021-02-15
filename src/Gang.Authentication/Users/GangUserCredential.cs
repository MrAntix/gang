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
            DateTimeOffset created,
            DateTimeOffset? validated = null
            )
        {
            Id = id;
            PublicKey = publicKey;
            Transports = transports?.ToImmutableList();
            Created = created;
            Validated = validated ?? created;
        }

        public string Id { get; }
        public GangPublicKey PublicKey { get; }
        public IImmutableList<string> Transports { get; }
        public DateTimeOffset Created { get; }
        public DateTimeOffset Validated { get; }
        public GangUserCredential SetValidated()
        {
            return new GangUserCredential(
                Id,
                PublicKey,
                Transports,
                Created,
                DateTimeOffset.UtcNow
                );
        }

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
