using Gang.Tasks;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gang.WebSockets
{
    public class WebSocketGangMember : IGangMember
    {
        readonly TaskQueue _sendQueue;
        readonly WebSocket _webSocket;
        readonly ArraySegment<byte> _buffer;
        readonly byte[] _id;

        public WebSocketGangMember(
            WebSocket webSocket)
        {
            _webSocket = webSocket;

            _sendQueue = new TaskQueue();
            _buffer = new ArraySegment<byte>(new byte[1024 * 4]);
            _id = Encoding.UTF8.GetBytes($"{Guid.NewGuid():N}");
        }

        byte[] IGangMember.Id { get { return _id; } }

        bool IGangMember.IsConnected => _webSocket.State == WebSocketState.Open;

        async Task<byte[]> IGangMember.ReceiveAsync()
        {
            var data = new MemoryStream();
            var result = default(WebSocketReceiveResult);
            do
            {
                result = await _webSocket
                   .ReceiveAsync(_buffer, CancellationToken.None);

                if (result.MessageType != WebSocketMessageType.Binary) return null;

                await data.WriteAsync(_buffer.Array, 0, result.Count);

            } while (!result.EndOfMessage);

            return data.ToArray();
        }

        async Task IGangMember.SendAsync(GangMessageTypes type, byte[] data)
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

        async Task IGangMember.DisconnectAsync(string reason)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, reason, CancellationToken.None);
        }
    }
}
