using Gang.Authentication.Users;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Gang.Demo.Web.Services.State
{
    public sealed class UserStore :
        IGangAuthenticationUserStore
    {
        IImmutableList<GangUserData> _users
            = ImmutableList<GangUserData>.Empty;

        public Task<GangUserData> TryGetByIdAsync(string id)
        {
            return Task.FromResult(
                _users.TryGetById(id)
                );
        }

        public Task SetAsync(GangUserData value)
        {
            lock (_users)
                _users = _users
                        .TryRemoveById(value.Id)
                        .Append(value)
                        .ToImmutableList();

            return Task.CompletedTask;
        }

        public Task<GangUserData> TryGetByEmailAddressAsync(string emailAddress)
        {
            return Task.FromResult(
                _users.FirstOrDefault(u => u.EmailAddress.Equals(emailAddress,
                        StringComparison.InvariantCultureIgnoreCase)
                    )
                );
        }

        public Task<GangUserData> TryGetByLinkTokenAsync(string token)
        {
            return Task.FromResult(
                _users.FirstOrDefault(u => u.LinkCode?.Value == token)
                );
        }
    }
}
