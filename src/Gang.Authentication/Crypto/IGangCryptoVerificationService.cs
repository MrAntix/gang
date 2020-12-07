using Gang.Authentication.Api;
using System;

namespace Gang.Authentication.Crypto
{
    public interface IGangCryptoVerificationService
    {
        public GangPublicKeyTypes KeyType { get; }
        public GangPublicKeyAlgorithm Algorithm { get; }
        bool Verify(
            GangCryptoParameters parameters,
            ReadOnlySpan<byte> data,
            ReadOnlySpan<byte> signature
            );
    }
}
