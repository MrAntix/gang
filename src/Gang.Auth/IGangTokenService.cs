using Gang.Contracts;
using System;

namespace Gang.Auth
{
    public interface IGangTokenService : IDisposable
    {
        string Create(GangTokenData data, string salt);
        GangTokenData TryDecode(string token);
        bool Verify(string token, string salt = null);
    }
}
