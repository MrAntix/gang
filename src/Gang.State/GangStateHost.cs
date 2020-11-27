using Gang.State.Commands;
using Gang.State.Storage;
using Gang.Tasks;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Gang.State
{
    public class GangStateHost<TStateData> :
        GangHostBase
        where TStateData : class, new()
    {
        readonly IGangCommandExecutor<TStateData> _executor;
        readonly IGangStateStore<TStateData> _store;
        readonly TaskQueue _tasks = new();

        public GangStateHost(
            IGangCommandExecutor<TStateData> executor,
            IGangStateStore<TStateData> store
           )
        {
            _executor = executor;
            _store = store;
        }

        public GangState<TStateData> State { get; private set; }

        protected override async Task OnConnectAsync()
        {
            State = await _store
                .RestoreAsync(Controller.GangId);
        }

        protected override async Task OnCommandAsync(
            byte[] bytes, GangAudit audit)
        {
            await _tasks.Enqueue(async () =>
            {
                var result = await _executor
                    .ExecuteAsync(State, bytes, audit);

                if (result.Errors == null)
                {
                    State = result;

                    await _store
                        .CommitAsync(Controller.GangId, State, audit);
                }
                else
                {
                    await OnCommandErrorAsync(result);
                }
            });
        }

        protected virtual Task OnCommandErrorAsync(GangState<TStateData> result)
        {
            Debug.WriteLine("Command Errors");

            return Task.CompletedTask;
        }
    }
}
