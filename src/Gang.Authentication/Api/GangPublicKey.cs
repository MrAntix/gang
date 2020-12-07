using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gang.Authentication.Api
{
    public sealed class GangPublicKey
    {
        public GangPublicKey(
            GangPublicKeyTypes keyType, GangPublicKeyAlgorithm algorithm,
            IDictionary<string, string> parameters)
        {
            KeyType = keyType;
            Algorithm = algorithm;
            Parameters = parameters
                ?.ToImmutableDictionary()
                ?? throw new ArgumentNullException(nameof(parameters));
        }

        public GangPublicKeyTypes KeyType { get; }
        public GangPublicKeyAlgorithm Algorithm { get; }
        public ImmutableDictionary<string, string> Parameters { get; }
    }
}
