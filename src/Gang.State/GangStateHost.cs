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
        readonly IGangStateStore _store;
        readonly TaskQueue _tasks = new();

        public GangStateHost(
            IGangCommandExecutor<TStateData> executor,
            IGangStateStore store
           )
        {
            _executor = executor;
            _store = store;
        }

        public GangState<TStateData> State { get; private set; }
        public async Task SetState(
            GangState<TStateData> result, GangAudit audit = null)
        {
            if (result.Errors == null)
            {
                State = await OnStateAsync(result);

                State = await _store
                    .CommitAsync(Controller.GangId, State, audit);
            }
            else
            {
                await OnCommandErrorAsync(result, audit);
            }
        }

        protected virtual Task<GangState<TStateData>> OnStateAsync(
            GangState<TStateData> state)
        {
            return Task.FromResult(
                    state
                );
        }

        protected override async Task OnConnectAsync()
        {
            State = await _store
                    .RestoreAsync<TStateData>(Controller.GangId)
                ?? new GangState<TStateData>();
        }

        protected override async Task OnCommandAsync(
            byte[] bytes, GangAudit audit)
        {
            await _tasks.Enqueue(async () =>
            {
                var result = await _executor
                    .ExecuteAsync(State, bytes, audit);

                await SetState(result, audit);
            });
        }

        protected virtual Task OnCommandErrorAsync(
            GangState<TStateData> result, GangAudit audit)
        {
            Debug.WriteLine("Command Errors");

            return Task.CompletedTask;
        }
    }
}
