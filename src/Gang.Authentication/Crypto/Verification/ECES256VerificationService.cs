using Gang.Authentication.Api;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Gang.Authentication.Crypto.Verification
{
    public class ECES256VerificationService : IGangCryptoVerificationService
    {
        GangPublicKeyTypes IGangCryptoVerificationService.KeyType => GangPublicKeyTypes.EC;
        GangPublicKeyAlgorithm IGangCryptoVerificationService.Algorithm => GangPublicKeyAlgorithm.Es256;

        bool IGangCryptoVerificationService.Verify(
            GangCryptoParameters parameters,
            ReadOnlySpan<byte> data,
            ReadOnlySpan<byte> signature
            )
        {
            if (parameters is null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var curveType = parameters.GetInt("-1");
            var curve = curveType switch
            {
                1 => ECCurve.NamedCurves.nistP256,
                _ => throw new Exception($"Unknown curve type {curveType}"),
            };

            var ecDsa = ECDsa.Create(new ECParameters
            {
                Curve = curve,
                Q = new ECPoint
                {
                    X = parameters.GetByteArray("-2"),
                    Y = parameters.GetByteArray("-3")
                }
            });

            using var ms = new MemoryStream(signature.ToArray());

            var header = ms.ReadByte();
            var b1 = ms.ReadByte();

            var markerR = ms.ReadByte();
            var b2 = ms.ReadByte();
            var vr = new byte[b2];
            ms.Read(vr, 0, vr.Length);
            vr = RemoveNegativeFlag(vr);

            var markerS = ms.ReadByte();
            var b3 = ms.ReadByte();
            var vs = new byte[b3];
            ms.Read(vs, 0, vs.Length);
            vs = RemoveNegativeFlag(vs);

            var parsedSignature = new byte[vr.Length + vs.Length];
            vr.CopyTo(parsedSignature, 0);
            vs.CopyTo(parsedSignature, vr.Length);

            return ecDsa.VerifyData(
                data.ToArray(),
                parsedSignature,
                HashAlgorithmName.SHA256);
        }

        static byte[] RemoveNegativeFlag(byte[] input)
        {
            return input[0] == 0 ? input[1..].ToArray() : input;
        }
    }
}
