using Gang.State;
using Gang.State.Events;
using Gang.State.Storage;
using Gang.Tests.State.Todos;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Gang.Tests.State.Fakes
{
    public sealed class FakeStateStore :
        IGangStateStore
    {
        public Task<GangState<TStateData>> CommitAsync<TStateData>(
            string gangId, GangState<TStateData> state, GangAudit audit)
            where TStateData : class
        {
            var typedState = state as GangState<TodosState>;
            CommitCalls = CommitCalls.Add(
                new CommitCall(gangId, typedState, audit));

            return Task.FromResult(
                new GangState<TStateData>(
                    typedState.Data as TStateData,
                    typedState.Version
                    )
                );
        }

        public IImmutableList<CommitCall> CommitCalls { get; private set; } = ImmutableList<CommitCall>.Empty;
        public sealed class CommitCall
        {
            public CommitCall(
                string gangId, GangState<TodosState> state, GangAudit audit)
            {
                GangId = gangId;
                State = state;
                Audit = audit;
            }

            public string GangId { get; }
            public GangState<TodosState> State { get; }
            public GangAudit Audit { get; }
        }


        Task<GangState<TStateData>> IGangStateStore.RestoreAsync<TStateData>(
            string gangId, TStateData initial
            )
        {
            RestoreCalls = RestoreCalls.Add(new RestoreCall(gangId));

            return Task.FromResult(
                new GangState<TStateData>(initial)
                );
        }

        public IImmutableList<RestoreCall> RestoreCalls { get; private set; } = ImmutableList<RestoreCall>.Empty;
        public sealed class RestoreCall
        {
            public RestoreCall(
                string gangId)
            {
                GangId = gangId;
            }

            public string GangId { get; }
        }

        IDisposable IGangStateStore.Subscribe(Func<IGangStateEvent, Task> observer, uint? startSequenceNumber)
        {
            throw new NotImplementedException();
        }
    }
}
