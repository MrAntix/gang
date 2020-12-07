using Gang.Authentication.Api;
using System;

namespace Gang.Authentication.Crypto
{
    public interface IGangCryptoService
    {
        string GetRandom(int parts, char separator = default);
        string Hash(string value, string salt);
        bool VerifySignature(
            GangPublicKey publicKey,
            ReadOnlySpan<byte> data,
            ReadOnlySpan<byte> signature);
    }
}
