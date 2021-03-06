using System;

namespace Gang.Authentication.Tokens
{
    public interface IGangTokenService : IDisposable
    {
        string Create(GangTokenData data, string salt);
        GangTokenData TryDecode(string token);
        bool Verify(string token, string salt = null);
    }
}
