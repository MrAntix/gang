using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gang.Authentication.Api
{
    public sealed class GangCredentialRegistration
    {
        public GangCredentialRegistration(
            string credentialId,
            GangPublicKey publicKey,
            IEnumerable<string> transports,
            string challenge
            )
        {
            CredentialId = credentialId ?? throw new System.ArgumentNullException(nameof(credentialId));
            PublicKey = publicKey ?? throw new System.ArgumentNullException(nameof(publicKey));
            Challenge = challenge;
            Transports = transports?.ToImmutableList();
        }

        public string CredentialId { get; }
        public GangPublicKey PublicKey { get; }
        public IImmutableList<string> Transports { get; }
        public string Challenge { get; }
    }

    public sealed class GangLink
    {
        public GangLink(
            string email,
            string code
            )
        {
            Email = email;
            Code = code;
        }

        public string Email { get; }
        public string Code { get; }
    }
}
