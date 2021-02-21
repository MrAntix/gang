using Gang.Commands;
using Gang.State.Commands;
using Gang.State.Storage;
using Gang.Tasks;
using System.Collections.Immutable;
using System.Diagnostics;
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

        //public GangState<TStateData> State { get; private set; }

        public Task<GangState<TStateData>> StateAsync { get; private set; }

        public async Task SetStateAsync(
            GangState<TStateData> state, GangAudit audit = null)
        {
            if (state == null) return; // state not loaded yet

            if (state.Errors == null)
            {
                var notifications = state.Notifications;

                state = await _store
                    .CommitAsync(
                        Controller.GangId,
                       await OnStateAsync(state),
                       audit
                       );

                StateAsync = Task.FromResult(state);

                if (notifications.Any())
                {
                    var members = Controller.GetGang().Members;

                    foreach (var notification in notifications)
                    {
                        var memberIds = members
                            .Where(m =>
                                m.Session?.User?.Id != null
                                && notification.UserIds.Contains(m.Session.User.Id)
                            )
                            .Select(m => m.Id)
                            .ToImmutableArray();

                        await Controller.SendCommandAsync(
                            notification.Command,
                            memberIds
                            );
                    }
                }
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

                var result = await _executor
                    .ExecuteAsync(state, bytes, audit);

                await SetStateAsync(result, audit);
            });
        }

        protected async Task ExecuteCommandAsync<TCommandData>(
            TCommandData data, GangAudit audit = null)
        {
            await _tasks.Enqueue(async () =>
            {
                var state = await StateAsync;

                audit ??= new GangAudit(Controller.GangId, state.Version, Id);
                var newState = await _executor
                    .ExecuteAsync(state, data, audit);

                await SetStateAsync(newState, audit);
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
