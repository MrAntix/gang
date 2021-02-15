using Gang.Events;
using Gang.Management;
using Gang.Management.Events;
using System;
using System.Threading.Tasks;

namespace Gang.Demo.Web.Server.Events
{
    public sealed class GangAddedHandler :
        IGangEventHandler<GangManagerEvent<GangAdded>>
    {
        readonly IGangManager _manager;
        readonly Func<HostMember> _createHost;

        public GangAddedHandler(
            IGangManager manager,
            Func<HostMember> createHost)
        {
            _manager = manager;
            _createHost = createHost;
        }

        async Task IGangEventHandler<GangManagerEvent<GangAdded>>
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
