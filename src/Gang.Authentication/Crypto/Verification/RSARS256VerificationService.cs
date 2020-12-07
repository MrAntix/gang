using Gang.Authentication.Api;
using System;
using System.Security.Cryptography;

namespace Gang.Authentication.Crypto.Verification
{
    public class RSARS256VerificationService : IGangCryptoVerificationService
    {
        GangPublicKeyTypes IGangCryptoVerificationService.KeyType => GangPublicKeyTypes.RSA;
        GangPublicKeyAlgorithm IGangCryptoVerificationService.Algorithm => GangPublicKeyAlgorithm.Rs256;

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

            var rsa = RSA.Create();

            rsa.ImportParameters(new RSAParameters
            {
                Exponent = parameters.GetByteArray("-2"),
                Modulus = parameters.GetByteArray("-1")
            });

            return rsa.VerifyData(
                 data.ToArray(),
                 signature.ToArray(),
                 HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1
                 );
        }
    }
}
