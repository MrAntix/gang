using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gang.Tests
{
    public class FakeGangMember : IGangMember
    {
        public FakeGangMember(
            string id, int delay = 50)
        {
            Id = Encoding.UTF8.GetBytes(id);
            Sent = new List<Tuple<GangMessageTypes, byte[]>>();

            Task.Delay(delay)
                .ContinueWith((_) => DisconnectAsync("delay").GetAwaiter().GetResult());
        }

        public byte[] Id { get; }

        public Func<Task> OnDisconnectAsync { get; private set; }
        public Action<GangMemberSendAsync> OnConnect { get; set; }


        Task IGangMember.ConnectAsync(
            IGangController controller, Func<Task> onDisconnectAsync)
        {
            OnDisconnectAsync = onDisconnectAsync;
            OnConnect?.Invoke(controller.SendAsync);
            return Task.CompletedTask;
        }

        public async Task DisconnectAsync(string reason = null)
        {
            if (OnDisconnectAsync != null)
            {
                await OnDisconnectAsync();
                OnDisconnectAsync = null;
            }
        }

        Task IGangMember.SendAsync(GangMessageTypes type, byte[] data, byte[] memberId)
        {
            Sent.Add(Tuple.Create(type, data));
            return Task.CompletedTask;
        }

        public IList<Tuple<GangMessageTypes, byte[]>> Sent { get; }

        void IDisposable.Dispose()
        {
            DisconnectAsync("disposed").GetAwaiter().GetResult();
        }
    }
}
