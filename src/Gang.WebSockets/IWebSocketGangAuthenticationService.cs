using Gang.Authentication;
using System;
using System.Threading.Tasks;

namespace Gang.WebSockets
{
    public interface IWebSocketGangAuthenticationService
    {
        Task ExecuteAsync(
            GangParameters parameters,
            Func<GangAuth, Task<IGangMember>> getMemberAsync);
    }
}
