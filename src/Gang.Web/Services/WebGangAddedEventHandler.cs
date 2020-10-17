using Gang.Contracts;
using Gang.Management;
using Gang.Management.Events;
using System;
using System.Threading.Tasks;

namespace Gang.Web.Services
{
    public class WebGangAddedEventHandler : GangManagerEventHandlerBase<GangAddedManagerEvent>
    {
        readonly IGangManager _handler;
        readonly Func<WebGangHost> _createHost;

        public WebGangAddedEventHandler(
            IGangManager handler,
            Func<WebGangHost> createHost)
        {
            _handler = handler;
            _createHost = createHost;
        }

        protected async override Task HandleAsync(GangAddedManagerEvent e)
        {
            var host = _createHost();

            await _handler.ManageAsync(
                new GangParameters(e.GangId, null),
                host);
        }
    }

    public class WebGangMemberAddedEventHandler : GangManagerEventHandlerBase<GangMemberAddedManagerEvent>
    {
        readonly IGangManager _handler;

        public WebGangMemberAddedEventHandler(
            IGangManager handler)
        {
            _handler = handler;
        }

        protected async override Task HandleAsync(GangMemberAddedManagerEvent e)
        {
            var host = _handler.GangById(e.GangId).HostMember;

            //await _handler.HandleAsync(
            //    new GangParameters(e.GangId, null),
            //    host);
        }
    }
}
