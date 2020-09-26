using Gang.Tasks;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Gang.WebSockets
{
    public class WebSocketGangMember : IGangMember
    {
        readonly TaskQueue _sendQueue;
        readonly WebSocket _webSocket;
        readonly ArraySegment<byte> _buffer;

        public WebSocketGangMember(
            byte[] id,
            WebSocket webSocket)
        {
            Id = id;
            _webSocket = webSocket;
            _sendQueue = new TaskQueue();
            _buffer = new ArraySegment<byte>(new byte[1024 * 4]);
        }

        public byte[] Id { get; }

        Task IGangMember.ConnectAsync(
            IGangController controller, Func<Task> _onDisconnectAsync)
        {
            return Task.Run(async () =>
            {
                do
                {
                    using var data = new MemoryStream();
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await _webSocket
                           .ReceiveAsync(_buffer, CancellationToken.None);

                        if (result.MessageType != WebSocketMessageType.Binary) break;

                        await data.WriteAsync(_buffer, CancellationToken.None);

                    } while (!result.EndOfMessage);

                    await controller.SendAsync(data.ToArray());

                } while (_webSocket.State == WebSocketState.Open);

                if (_onDisconnectAsync != null)
                    await _onDisconnectAsync();
            });
        }

        public async Task DisconnectAsync(string reason)
        {
            if (_webSocket.State == WebSocketState.Open)
                await _webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure, reason, CancellationToken.None);
        }

        async Task IGangMember.SendAsync(GangMessageTypes type, byte[] data, byte[] _, short? sequenceNumber)
        {
            await _sendQueue.Enqueue(async () =>
            {
                try
                {
                    await _webSocket.SendAsync(
                        new ArraySegment<byte>(new[] { (byte)type }),
                        WebSocketMessageType.Binary, data == null, CancellationToken.None);

                    if (sequenceNumber != null)
                    {
                        await _webSocket.SendAsync(
                            BitConverter.GetBytes(sequenceNumber.Value),
                            WebSocketMessageType.Binary, data == null, CancellationToken.None);
                    }

                    if (data != null)
                    {
                        await _webSocket.SendAsync(
                            new ArraySegment<byte>(data),
                            WebSocketMessageType.Binary, true, CancellationToken.None);
                    }
                }
                catch (WebSocketException)
                {
                }
            });
        }

        void IDisposable.Dispose()
        {
            DisconnectAsync("disposed").GetAwaiter().GetResult();
            _webSocket.Dispose();
        }
    }
}
