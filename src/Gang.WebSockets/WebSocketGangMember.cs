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

        async Task DisconnectAsync(string reason = "disconnected")
        {
            if (_webSocket.State == WebSocketState.Open)
                await _webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure, reason, CancellationToken.None);
        }

        public byte[] Id { get; }

        async Task IGangMember.ConnectAsync(
            string gangId,
            GangMemberSendAsync sendAsync)
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

                    await data.WriteAsync(_buffer.Array, 0, result.Count);

                } while (!result.EndOfMessage);

                await sendAsync(data.ToArray());

            } while (_webSocket.State == WebSocketState.Open);
        }

        async Task IGangMember.DisconnectAsync(string reason)
        {
            await DisconnectAsync(reason);
        }

        async Task IGangMember.SendAsync(GangMessageTypes type, byte[] data, byte[] _)
        {
            await _sendQueue.Enqueue(async () =>
            {
                try
                {
                    await _webSocket.SendAsync(
                        new ArraySegment<byte>(new[] { (byte)type }),
                        WebSocketMessageType.Binary, data == null, CancellationToken.None);

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
