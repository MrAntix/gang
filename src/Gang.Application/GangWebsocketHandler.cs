using Antix.Gang;
using Gang.Contracts;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Gang.Application
{
    public class GangWebsocketHandler :
        IGangWebsocketHandler
    {
        Task IGangWebsocketHandler.HandleAsync(
            WebSocket webSocket, GangParameters parameters)
        {
            throw new NotImplementedException();
        }
    }
}
