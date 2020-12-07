using Gang.Authentication;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Gang.Demo.Web.Services
{
    public sealed class SimpleAuthenticationHandler :
        IGangAuthenticationHandler
    {
        IImmutableDictionary<string, string> _userIds = ImmutableDictionary<string, string>.Empty;

        Task<GangSession> IGangAuthenticationHandler
            .HandleAsync(GangParameters parameters)
        {
            var token = parameters.Token ?? $"{Guid.NewGuid():N}";

            if (!_userIds.TryGetValue(token, out var userId))
            {
                userId = $"{Guid.NewGuid():N}";
                _userIds = _userIds.Add(token, userId);
            }

            return Task.FromResult(
                new GangSession(userId, token: token)
                );
        }
    }
}
