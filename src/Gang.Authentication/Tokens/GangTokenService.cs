using Gang.Serialization;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Gang.Authentication.Tokens
{
    public sealed class GangTokenService :
        IGangTokenService
    {
        readonly HMACSHA256 _hasher;
        readonly IGangSerializationService _serializationService;

        public GangTokenService(
            GangAuthenticationSettings settings,
            IGangSerializationService serializationService)
        {
            _serializationService = serializationService;

            _hasher = new HMACSHA256(
                Encoding.UTF8.GetBytes(settings.Secret)
             );
        }

        string IGangTokenService.Create(GangTokenData data, string salt)
        {
            var serialized = _serializationService.Serialize(data);
            var encodedData = Base64Encode(serialized);

            return $"{encodedData}.{Hash(encodedData, salt)}";
        }

        GangTokenData IGangTokenService.TryDecode(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var tokenParts = token.Split('.');
            if (tokenParts.Length != 2)
                return null;

            var data = tokenParts[0];

            return _serializationService
                 .Deserialize<GangTokenData>(Base64Decode(data));
        }

        bool IGangTokenService.Verify(string token, string Salt)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            var tokenParts = token.Split('.');
            if (tokenParts.Length != 2)
                return false;

            var data = tokenParts[0];
            var signature = tokenParts[1];

            var hash = Hash(data, Salt);

            return hash == signature;
        }

        string Hash(string text, string salt = null)
        {
            var textBytes = Encoding.UTF8
                .GetBytes(string.Concat(text, salt));

            var hashedBytes = _hasher.ComputeHash(textBytes);

            return Base64Encode(hashedBytes);
        }

        /// <summary>
        /// Encodes the specified byte array.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        public static string Base64Encode(byte[] arg)
        {
            var s = Convert.ToBase64String(arg); // Standard base64 encoder

            s = s.Split('=')[0]; // Remove any trailing '='s
            s = s.Replace('+', '-'); // 62nd char of encoding
            s = s.Replace('/', '_'); // 63rd char of encoding

            return s;
        }

        /// <summary>
        /// Decodes the specified string.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Illegal base64url string!</exception>
        public static byte[] Base64Decode(string arg)
        {
            var s = arg;
            s = s.Replace('-', '+'); // 62nd char of encoding
            s = s.Replace('_', '/'); // 63rd char of encoding

            switch (s.Length % 4) // Pad with trailing '='s
            {
                case 0:
                    break; // No pad chars in this case
                case 2:
                    s += "==";
                    break; // Two pad chars
                case 3:
                    s += "=";
                    break; // One pad char
                default:
                    throw new Exception("Illegal base64url string!");
            }

            return Convert.FromBase64String(s); // Standard base64 decoder
        }
        void IDisposable.Dispose()
        {
            _hasher.Dispose();
        }
    }
}
