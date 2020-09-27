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
            MessagesReceived = new List<Message>();
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

        Task IGangMember.SendAsync(GangMessageTypes type,
            byte[] data, byte[] memberId, uint? sequenceNumber)
        {
            MessagesReceived.Add(new Message(type, data, sequenceNumber));
            return Task.CompletedTask;
        }

        public IList<Message> MessagesReceived { get; }

        void IDisposable.Dispose()
        {
            DisconnectAsync("disposed").GetAwaiter().GetResult();
        }

        public class Message
        {
            public Message(
                GangMessageTypes type, byte[] data, uint? sequenceNumber)
            {
                Type = type;
                Data = data;
                SequenceNumber = sequenceNumber;
            }

            public GangMessageTypes Type { get; }
            public byte[] Data { get; }
            public uint? SequenceNumber { get; }
        }
    }
}
