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
            Sent = new List<Tuple<GangMessageTypes, byte[]>>();
            _connected = new TaskCompletionSource<bool>(delay);

            Task.Delay(delay).ContinueWith((_) => Disconnect());
        }

        public byte[] Id { get; }

        public Action<GangMemberSendAsync> OnConnect { get; set; }

        void Disconnect()
        {
            if (!_connected.Task.IsCompleted)
                _connected.SetResult(true);
        }

        async Task IGangMember.ConnectAndBlockAsync(IGangController controller)
        {
            OnConnect?.Invoke(controller.SendAsync);
            await _connected.Task;
        }

        Task IGangMember.DisconnectAsync(string reason)
        {
            Disconnect();
            return Task.CompletedTask;
        }

        Task IGangMember.SendAsync(GangMessageTypes type, byte[] data, byte[] memberId)
        {
            Sent.Add(Tuple.Create(type, data));
            return Task.CompletedTask;
        }

        public IList<Tuple<GangMessageTypes, byte[]>> Sent { get; }

        void IDisposable.Dispose()
        {
            Disconnect();
        }
    }
}
