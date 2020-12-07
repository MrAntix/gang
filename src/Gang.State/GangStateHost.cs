using Gang.State.Commands;
using Gang.State.Storage;
using Gang.Tasks;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Gang.State
{
    public class GangStateHost<TStateData> :
        GangHostBase
        where TStateData : class
    {
        readonly IGangCommandExecutor<TStateData> _executor;
        readonly IGangStateStore _store;
        readonly TStateData _initialState;
        readonly TaskQueue _tasks = new();

        public GangStateHost(
            IGangCommandExecutor<TStateData> executor,
            IGangStateStore store,
            TStateData initialState
           )
        {
            _executor = executor;
            _store = store;
            _initialState = initialState;
        }

        public GangState<TStateData> State { get; private set; }
        public async Task SetState(
            GangState<TStateData> state, GangAudit audit = null)
        {
            if (state == null) return; // state not loaded yet

            if (state.Errors == null)
            {
                State = await OnStateAsync(state);

                State = await _store
                    .CommitAsync(Controller.GangId, State, audit);
            }
            else
            {
                await OnCommandErrorAsync(state, audit);
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
            State = await OnStateAsync(
                await _store.RestoreAsync(Controller.GangId, _initialState)
                );
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
