using Gang.Authentication.Api;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Gang.Authentication.Crypto
{
    public sealed class GangCryptoService : IGangCryptoService
    {
        readonly byte[] _secretBytes;
        readonly IImmutableDictionary<Tuple<GangPublicKeyTypes, GangPublicKeyAlgorithm>, IGangCryptoVerificationService> _verifiers;

        public const string VALID_CHARS = "0123456789";
        public const char NON_BREAKING_SPACE = ' ';

        public GangCryptoService(
            IGangCryptoSettings settings,
            IEnumerable<IGangCryptoVerificationService> verifiers)
        {
            _secretBytes = Encoding.UTF8.GetBytes(settings.Secret);
            _verifiers = verifiers
                ?.ToImmutableDictionary(s => Tuple.Create(s.KeyType, s.Algorithm));
        }

        string IGangCryptoService.GetRandom(int parts, char separator)
        {
            if (parts < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(parts), $"{nameof(parts)} should be 1 or more");
            }

            using var random = new RNGCryptoServiceProvider();

            var hasSeparator = separator != default;
            var length = hasSeparator ? 4 * parts - 1 : 3 * parts;

            var data = new byte[length];
            random.GetBytes(data);

            return new string(
                data.Select(
                    (c, i) =>
                        hasSeparator && (i % 4 == 3)
                         ? separator
                         : VALID_CHARS[c % VALID_CHARS.Length]
                    ).ToArray()
                );
        }

        string IGangCryptoService.Hash(string value, string salt)
        {
            var valueWithSalt = string.Concat(value, salt);
            var valueWithSaltBytes = Encoding.UTF8.GetBytes(valueWithSalt);

            using var hasher = new HMACSHA256(_secretBytes);

            var hashedBytes = hasher.ComputeHash(valueWithSaltBytes);

            return Convert.ToBase64String(hashedBytes);
        }

        bool IGangCryptoService.VerifySignature(
            GangPublicKey publicKey,
            ReadOnlySpan<byte> data, ReadOnlySpan<byte> signature)
        {
            var verifierKey = Tuple.Create(publicKey.KeyType, publicKey.Algorithm);
            if (!_verifiers.ContainsKey(verifierKey))
            {
                throw new NotSupportedException($"KeyType {publicKey.KeyType} / algorithm {publicKey.Algorithm} combination is not supported");
            }

            var verifier = _verifiers[verifierKey];
            var parameters = new GangCryptoParameters(this, publicKey.Parameters);

            return verifier.Verify(parameters, data, signature);
        }
    }
}
