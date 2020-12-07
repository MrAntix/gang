using System.Threading.Tasks;

namespace Gang.Management
{
    public sealed class GangMemberConnectionState
    {
        public GangMemberConnectionState(
            bool disconnect = false)
        {
            _connected = new TaskCompletionSource<bool>();
            BlockingTask = _connected.Task;

            if(disconnect) SetDisconnected();
        }

        readonly TaskCompletionSource<bool> _connected;

        public Task BlockingTask { get; }

        public void SetDisconnected()
        {
            _connected.SetResult(true);
        }

        public static readonly GangMemberConnectionState Disconnected            =new(true);
    }
}