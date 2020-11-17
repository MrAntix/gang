using Gang.Authentication.Contracts;
using System;

namespace Gang.Authentication.Services
{
    public interface IGangTokenService : IDisposable
    {
        string Create(GangTokenData data, string salt);
        GangTokenData TryDecode(string token);
        bool Verify(string token, string salt = null);
    }
}
