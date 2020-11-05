using Gang.Contracts;
using Gang.Management;
using System;
using System.Threading.Tasks;

namespace Gang.WebSockets
{
    public sealed class WebSocketGangAuthenticator :
        IWebSocketGangAutherticator
    {
        public const string RESULT_DENIED = "denied";

        readonly GangAuthenticationFunc _authenticateAsync;
        readonly IGangManager _manager;

        public WebSocketGangAuthenticator(
            GangAuthenticationFunc authenticateAsync,
            IGangManager manager)
        {
            _authenticateAsync = authenticateAsync;
            _manager = manager;
        }

        async Task IWebSocketGangAutherticator.ExecuteAsync(
            GangParameters parameters,
            Func<GangAuth, Task<IGangMember>> getMemberAsync)
        {
            var auth = await _authenticateAsync(parameters);
            using var gangMember = await getMemberAsync(auth);

            if (auth?.Id == null)
                await gangMember.DisconnectAsync(RESULT_DENIED);

            else
                await _manager.ManageAsync(parameters, gangMember).BlockAsync();
        }
    }
}
