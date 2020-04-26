using System;
using System.Text;
using System.Threading.Tasks;

namespace Gang
{
    public abstract class GangHostMemberBase : IGangMember
    {
        public byte[] Id { get; } = Encoding.UTF8.GetBytes("HOST");

        readonly TaskCompletionSource<bool> _connected;

        public GangHostMemberBase()
        {
            _connected = new TaskCompletionSource<bool>();
        }

        protected IGangController Controller { get; private set; }
        protected virtual Task OnMemberConnectAsync(byte[] memberId) => Task.CompletedTask;
        protected virtual Task OnMemberDisconnectAsync(byte[] memberId) => Task.CompletedTask;
        protected virtual Task OnCommandAsync(byte[] data, GangMessageAudit audit) => Task.CompletedTask;
        protected virtual Task OnDisconnectAsync() => Task.CompletedTask;

        async Task IGangMember.ConnectAndBlockAsync(
            IGangController controller)
        {
            Controller = controller;
            await _connected.Task;
        }

        async Task IGangMember.SendAsync(
            GangMessageTypes type,
            byte[] data, byte[] memberId)
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
                    await OnCommandAsync(data, new GangMessageAudit(memberId ?? Id));

                    break;
            }
        }

        public async Task DisconnectAsync(string reason = null)
        {
            await OnDisconnectAsync();

            if (!_connected.Task.IsCompleted)
                _connected.SetResult(true);
        }

        void IDisposable.Dispose()
        {
            if (!_connected.Task.IsCompleted)
                DisconnectAsync("disposed").GetAwaiter().GetResult();
        }
    }
}
