using Gang.Contracts;
using Gang.Management;
using System;
using System.Threading.Tasks;

namespace Gang
{
    public abstract class GangHostBase : IGangMember
    {
        public byte[] Id { get; } = "HOST".GangToBytes();
        public GangAuth Auth { get; } = null;

        protected IGangController Controller { get; private set; }

        protected virtual Task OnConnectAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnMemberConnectAsync(byte[] memberId)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnMemberDisconnectAsync(byte[] memberId)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnCommandAsync(byte[] bytes, GangAudit audit)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnDisconnectAsync()
        {
            return Task.CompletedTask;
        }

        Func<Task> _onDisconnectAsync;

        async Task IGangMember.ConnectAsync(
            IGangController controller, Func<Task> onDisconnectAsync)
        {
            _onDisconnectAsync = onDisconnectAsync;
            Controller = controller;

            await OnConnectAsync();
        }

        async Task IGangMember.SendAsync(
            GangMessageTypes type,
            byte[] data, byte[] memberId, uint? sequenceNumber)
        {
            switch (type)
            {
                case GangMessageTypes.Connect:
                    await OnMemberConnectAsync(data);

                    break;
                case GangMessageTypes.Disconnect:
                    await OnMemberDisconnectAsync(data);

                    break;
                case GangMessageTypes.Command:
                    var audit = new GangAudit(Controller.GangId, memberId, sequenceNumber);
                    await OnCommandAsync(data, audit);

                    break;
            }
        }

        public async Task DisconnectAsync(string reason = null)
        {
            if (_onDisconnectAsync == null) return;

            await OnDisconnectAsync();
            await _onDisconnectAsync();
            _onDisconnectAsync = null;
        }

        void IDisposable.Dispose()
        {
            DisconnectAsync("disposed")
                .GetAwaiter().GetResult();

            GC.SuppressFinalize(this);
        }
    }
}
