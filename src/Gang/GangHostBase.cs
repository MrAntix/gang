using Gang.Authentication;
using Gang.Management;
using System;
using System.Threading.Tasks;

namespace Gang
{
    public abstract class GangHostBase : IGangMember
    {
        public byte[] Id { get; } = "HOST".GangToBytes();
        public GangSession Session { get; } = GangSession.Default;

        public IGangController Controller { get; private set; }

        protected virtual Task OnConnectAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnMemberConnectAsync(GangAudit audit)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnMemberDisconnectAsync(GangAudit audit)
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
            IGangController controller,
            Func<Task> onDisconnectAsync)
        {
            _onDisconnectAsync = onDisconnectAsync;
            Controller = controller;

            await OnConnectAsync();
        }

        async Task IGangMember.HandleAsync(
            GangMessageTypes type,
            byte[] data, GangAudit audit)
        {
            switch (type)
            {
                case GangMessageTypes.Connect:
                    await OnMemberConnectAsync(audit);

                    break;
                case GangMessageTypes.Disconnect:
                    await OnMemberDisconnectAsync(audit);

                    break;
                case GangMessageTypes.Command:
                    await OnCommandAsync(data, audit);
                    await Controller.SendAsync(
                        GangMessageTypes.Receipt,
                        BitConverter.GetBytes(audit.Version.Value),
                        new[] { audit.MemberId }
                        );

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
