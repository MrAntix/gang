using Gang.Authentication;
using Gang.Management;
using Gang.Tasks;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Gang.WebSockets
{
    public sealed class WebSocketGangMember : IGangMember
    {
        readonly TaskQueue _sendQueue;
        readonly WebSocket _webSocket;
        readonly ArraySegment<byte> _buffer;

        public WebSocketGangMember(
            byte[] id,
            GangSession session,
            WebSocket webSocket)
        {
            Id = id;
            Session = session;
            _webSocket = webSocket;
            _sendQueue = new TaskQueue();
            _buffer = new ArraySegment<byte>(new byte[1024 * 4]);
        }

        public byte[] Id { get; }
        public GangSession Session { get; }

        public IGangController Controller { get; private set; }

        Task IGangMember.ConnectAsync(
            IGangController controller, Func<Task> _onDisconnectAsync)
        {
            Controller = controller;

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

                        await data.WriteAsync(_buffer.AsMemory(0, result.Count));

                    } while (!result.EndOfMessage);

                    if (data.Length > 0)
                        await controller.ReceiveAsync(data.ToArray());

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

        async Task IGangMember.HandleAsync(GangMessageTypes type, byte[] data, GangAudit audit)
        {
            await _sendQueue.Enqueue(async () =>
            {
                try
                {
                    await _webSocket.SendAsync(
                        new[] { (byte)type },
                        WebSocketMessageType.Binary, data == null, CancellationToken.None);

                    if (audit?.Version != null)
                        await _webSocket.SendAsync(
                            BitConverter.GetBytes(audit.Version.Value),
                            WebSocketMessageType.Binary, data == null, CancellationToken.None);

                    if (data != null)
                        await _webSocket.SendAsync(
                            data,
                            WebSocketMessageType.Binary, true, CancellationToken.None);
                }
                catch (WebSocketException)
                {
                    // ignore this
                }
            });
        }

        void IDisposable.Dispose()
        {
            DisconnectAsync("disposed").GetAwaiter().GetResult();
            _webSocket.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
