using Gang.Contracts;
using Gang.Events;
using Gang.Web.Services.Events;
using Gang.Web.Services.State;
using System;
using System.Collections.Generic;
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

        protected async override Task HandleAsync(GangAddedEvent e)
        {
            var host = _getHost();

            await _handler.HandleAsync(
                new GangParameters(e.GangId, null),
                host);
        }
    }
}
