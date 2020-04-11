using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gang.Tests
{
    public class FakeGangMember : IGangMember
    {
        readonly TaskCompletionSource<bool> _connected;

        public FakeGangMember(
            string id, int delay = 50)
        {
            Id = Encoding.UTF8.GetBytes(id);
            Received = new List<Tuple<GangMessageTypes, byte[]>>();
            _connected = new TaskCompletionSource<bool>(delay);

            Task.Delay(delay).ContinueWith((_) => Disconnect());
        }

        public byte[] Id { get; }

        public Action<Func<byte[], Task>> OnConnect { get; set; }

        async Task IGangMember.ConnectAsync(Func<byte[], Task> onReceiveAsync)
        {
            OnConnect?.Invoke(onReceiveAsync);

            await _connected.Task;
        }

        Task IGangMember.DisconnectAsync(string reason)
        {
            _connected.SetResult(true);

            return Task.CompletedTask;
        }

        Task IGangMember.SendAsync(GangMessageTypes type, byte[] data, byte[] memberId)
        {
            Received.Add(Tuple.Create(type, data));
            return Task.CompletedTask;
        }

        public IList<Tuple<GangMessageTypes, byte[]>> Received { get; }

        void Disconnect()
        {
            _connected.SetResult(true);
        }

        void IDisposable.Dispose()
        {
            Disconnect();
        }

    }
}
