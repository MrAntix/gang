using Gang.Contracts;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Antix.Gang
{
    public interface IGangWebsocketHandler
    {
        Task HandleAsync(WebSocket webSocket, GangParameters parameters);
    }
}
