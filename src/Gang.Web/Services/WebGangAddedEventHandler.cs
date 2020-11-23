using Antix.Handlers;
using Gang.Management;
using Gang.Management.Events;
using System;
using System.Threading.Tasks;

namespace Gang.Web.Services
{
    public sealed class WebGangAddedEventHandler :
        IHandler<GangManagerEvent<GangAdded>>
    {
        readonly IGangManager _manager;
        readonly Func<WebGangHost> _createHost;

        public WebGangAddedEventHandler(
            IGangManager manager,
            Func<WebGangHost> createHost)
        {
            _manager = manager;
            _createHost = createHost;
        }

        async Task IHandler<GangManagerEvent<GangAdded>>
            .HandleAsync(GangManagerEvent<GangAdded> e)
        {
            var host = _createHost();

            await _manager.ManageAsync(
                new GangParameters(e.Audit.GangId),
                host
                );
        }
    }
}
