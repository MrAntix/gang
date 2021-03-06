using Gang.State.Commands;
using Gang.State.Storage;
using Gang.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
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
        readonly TaskCompletionSource<GangState<TStateData>> _stateLoaded;
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

            _stateLoaded = new TaskCompletionSource<GangState<TStateData>>();
            StateAsync = _stateLoaded.Task;
        }

        protected async Task QueueCommandAsync<TCommandData>(
            TCommandData data, GangAudit audit = null)
        {
            await _tasks.Enqueue(async () =>
            {
                var state = await StateAsync;

                audit ??= new GangAudit(Controller.GangId, state.Version, Id);
                var newState = await _executor
                    .ExecuteAsync(state, data, audit);

                if (newState.Errors?.Any() ?? false)
                    throw new GangStateCommandException(data, audit);

                await SetStateAsync(newState, audit);

                var results = newState.GetResults(audit);
                await OnCommandExecutedAsync(results, audit);
            });
        }

        public Task<GangState<TStateData>> StateAsync { get; private set; }

        public async Task SetStateAsync(
            GangState<TStateData> state, GangAudit audit = null)
        {
            if (state == null) return; // state not loaded yet

            if (state.Errors?.Any() == true) return;

            state = await _store
                .CommitAsync(
                    Controller.GangId,
                   await OnStateAsync(state),
                   audit
                   );

            StateAsync = Task.FromResult(state);
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
            var state = await OnStateAsync(
                await _store.RestoreAsync(Controller.GangId, _initialState)
                );
            _stateLoaded.SetResult(state);
        }

        protected override async Task OnCommandAsync(
            byte[] bytes, GangAudit audit)
        {
            await _tasks.Enqueue(async () =>
            {
                var state = await StateAsync;

                var newState = await _executor
                    .ExecuteAsync(state, bytes, audit);

                await SetStateAsync(newState, audit);

                var results = newState.GetResults(audit);
                await OnCommandExecutedAsync(results, audit);
            });
        }

        protected virtual Task OnCommandExecutedAsync(
            IEnumerable<GangStateNotification> results,
            GangAudit audit
            )
        {
            if (results?.Any() == true)
                Console.WriteLine($"Results:\n{string.Join("\n", results)}");

            return Task.CompletedTask;
        }
    }
}
