using Gang.Contracts;
using Gang.Events;
using System;
using System.Threading.Tasks;

namespace Gang.Web.Services
{
    public class WebGangAddedEventHandler : GangEventHandlerBase<GangAddedEvent>
    {
        readonly IGangHandler _handler;
        readonly Func<WebGangHost> _getHost;

        public WebGangAddedEventHandler(
            IGangHandler handler,
            Func<WebGangHost> getHost)
        {
            _handler = handler;
            _getHost = getHost;
        }

        public override async Task HandleAsync(GangAddedEvent e)
        {
            var host = _getHost();

            await _handler.HandleAsync(
                new GangParameters(e.GangId, null),
                host);
        }
    }
}
