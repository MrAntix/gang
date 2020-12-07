using Gang.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;

namespace Gang.Authentication.Crypto
{
    public class GangCryptoParameters
    {
        readonly IGangCryptoService _crypto;
        readonly IImmutableDictionary<string, string> _parameters;

        public GangCryptoParameters(
            IGangCryptoService crypto,
            IDictionary<string, string> parameters)
        {
            _crypto = crypto;
            _parameters = parameters
                ?.ToImmutableDictionary()
                ?? throw new ArgumentNullException(nameof(parameters));
        }

        public int GetInt(string key)
        {
            return TryGetInt(key).Value;
        }

        public int? TryGetInt(string key)
        {
            var value = _parameters[key];
            if (value == null)
            {
                return null;
            }

            return int.Parse(value, NumberStyles.Any);
        }

        public string GetString(string key)
        {
            return GangSerialization.Base64UrlToString(_parameters[key]);
        }

        public ReadOnlySpan<byte> GetBytes(string key)
        {
            return GangSerialization.Base64UrlToBytes(_parameters[key]);
        }

        public byte[] GetByteArray(string key)
        {
            return GetBytes(key).ToArray();
        }
    }

}
