using System.Threading.Tasks;

namespace Gang.Members
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

        internal void Disconnected()
        {
            _connected.SetResult(true);
        }
    }
}