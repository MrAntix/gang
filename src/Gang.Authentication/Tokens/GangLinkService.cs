using System.Linq;
using System.Security.Cryptography;

namespace Gang.Authentication.Tokens
{
    public class GangLinkService :
        IGangLinkService
    {
        public const string VALID_CHARS = "ABCDEFHJKLMNPQRTUVWXYZ34679";

        string IGangLinkService.CreateCode()
        {
            using var random = new RNGCryptoServiceProvider();

            var data = new byte[11];
            random.GetBytes(data);

            return new string(
                data.Select((c, i) => i % 4 == 3 ? '-' : VALID_CHARS[c % VALID_CHARS.Length]).ToArray()
                );
        }
    }
}
