using System.Threading.Tasks;

namespace Gang.Management
{
    public sealed class GangMemberConnectionState
    {
        public GangMemberConnectionState()
        {
            _connected = new TaskCompletionSource<bool>();
            BlockingTask = _connected.Task;
        }

        readonly TaskCompletionSource<bool> _connected;

        public Task BlockingTask { get; }

        public void Disconnected()
        {
            _connected.SetResult(true);
        }
    }
}