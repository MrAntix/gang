using Gang.Commands;
using Gang.Contracts;
using Gang.Management;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Gang
{
    public abstract class GangHostBase : IGangMember
    {
        public byte[] Id { get; } = Encoding.UTF8.GetBytes("HOST");

        protected IGangController Controller { get; private set; }

        protected virtual Task OnConnectAsync() => Task.CompletedTask;
        protected virtual Task OnMemberConnectAsync(byte[] memberId) => Task.CompletedTask;
        protected virtual Task OnMemberDisconnectAsync(byte[] memberId) => Task.CompletedTask;
        protected virtual Task OnCommandAsync(byte[] data, GangMessageAudit audit) => Task.CompletedTask;
        protected virtual Task OnDisconnectAsync() => Task.CompletedTask;

        Func<Task> _onDisconnectAsync;
        Func<byte[], GangMessageAudit, Task> _onCommandAsync;

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
                    var audit = new GangMessageAudit(Id, memberId, sequenceNumber);
                    await OnCommandAsync(data, audit);

                    if (_onCommandAsync != null) await _onCommandAsync(data, audit);

                    break;
            }
        }

        protected void Use<THost>(
            IGangCommandExecutor<THost> executor)
            where THost : GangHostBase
        {
            if (executor is null)
                throw new ArgumentNullException(nameof(executor));

            _onCommandAsync = (data, audit)
                => executor.ExecuteAsync(this as THost, data, audit);
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
