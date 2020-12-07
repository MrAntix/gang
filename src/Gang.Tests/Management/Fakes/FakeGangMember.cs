using Gang.Authentication;
using Gang.Management;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gang.Tests.Management.Fakes
{

    public sealed class FakeGangMember : IGangMember
    {
        public FakeGangMember(
             string id,
             GangSession session = null)
        {
            Id = id.GangToBytes();
            Session = session ?? GangSession.Default;
            MessagesReceived = new List<Message>();
        }

        public byte[] Id { get; }
        public GangSession Session { get; }

        public IGangController Controller { get; private set; }
        public Func<Task> OnDisconnectAsync { get; private set; }

        public bool Connected { get; private set; }
        public bool Disconnected { get; private set; }
        public string DisconnectedReason { get; private set; }


        Task IGangMember.ConnectAsync(
            IGangController controller, Func<Task> onDisconnectAsync)
        {
            Controller = controller;
            OnDisconnectAsync = onDisconnectAsync;
            Connected = true;
            return Task.CompletedTask;
        }

        public async Task DisconnectAsync(string reason = null)
        {
            if (OnDisconnectAsync != null)
            {
                await OnDisconnectAsync();
                OnDisconnectAsync = null;
            }
            Disconnected = true;
            DisconnectedReason = reason;
        }

        Task IGangMember.HandleAsync(GangMessageTypes type,
            byte[] data, GangAudit audit)
        {
            MessagesReceived.Add(new Message(type, data, audit?.Sequence));
            return Task.CompletedTask;
        }

        public IList<Message> MessagesReceived { get; }

        void IDisposable.Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public sealed class Message
        {
            public Message(
                GangMessageTypes type, byte[] data, uint? version)
            {
                Type = type;
                Data = data;
                Version = version;
            }

            public GangMessageTypes Type { get; }
            public byte[] Data { get; }
            public uint? Version { get; }
        }
    }
}
