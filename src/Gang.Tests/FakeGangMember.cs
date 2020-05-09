using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gang.Tests
{
    public class FakeGangMember : IGangMember
    {
        public FakeGangMember(
            string id)
        {
            Id = Encoding.UTF8.GetBytes(id);
            MessagesReceived = new List<Tuple<GangMessageTypes, byte[]>>();
        }

        public byte[] Id { get; }

        public IGangController Controller { get; private set; }
        public Func<Task> OnDisconnectAsync { get; private set; }


        Task IGangMember.ConnectAsync(
            IGangController controller, Func<Task> onDisconnectAsync)
        {
            Controller = controller;
            OnDisconnectAsync = onDisconnectAsync;
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
            MessagesReceived.Add(Tuple.Create(type, data));
            return Task.CompletedTask;
        }

        public IList<Tuple<GangMessageTypes, byte[]>> MessagesReceived { get; }

        void IDisposable.Dispose()
        {
            DisconnectAsync("disposed").GetAwaiter().GetResult();
        }
    }
}
