using Gang.Authentication.Users;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Gang.Web.Services.State
{
    public sealed class WebGangAuthenticationUserStore :
        IGangAuthenticationUserStore
    {
        IImmutableList<GangUser> _users
            = ImmutableList<GangUser>.Empty;

        public Task<GangUser> TryGetAsync(string id)
        {
            return Task.FromResult(
                _users.TryGetById(id)
                );
        }

        public Task SetAsync(GangUser value)
        {
            lock (_users)
                _users = _users
                        .TryRemoveById(value.Id)
                        .Append(value)
                        .ToImmutableList();

            return Task.CompletedTask;
        }

        public Task<GangUser> TryGetByEmailAddressAsync(string emailAddress)
        {
            return Task.FromResult(
                _users.FirstOrDefault(u => u.EmailAddress.Equals(emailAddress,
                        StringComparison.InvariantCultureIgnoreCase)
                    )
                );
        }

        public Task<GangUser> TryGetByLinkTokenAsync(string token)
        {
            return Task.FromResult(
                _users.FirstOrDefault(u => u.LinkToken?.Value == token)
                );
        }
    }
}
