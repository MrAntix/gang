using Gang.State;
using Gang.State.Storage;
using Gang.Tests.State.Todos;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Gang.Tests.State.Fakes
{
    public sealed class FakeStateStore :
        IGangStateStore<TodosState>
    {
        public Task CommitAsync(
            string gangId, GangState<TodosState> state, GangAudit audit)
        {
            CommitCalls = CommitCalls.Add(new CommitCall(gangId, state, audit));

            return Task.CompletedTask;
        }

        public IImmutableList<CommitCall> CommitCalls { get; private set; } = ImmutableList<CommitCall>.Empty;
        public class CommitCall
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


        public Task<GangState<TodosState>> RestoreAsync(
            string gangId)
        {
            RestoreCalls = RestoreCalls.Add(new RestoreCall(gangId));

            return Task.FromResult(
                new GangState<TodosState>()
                );
        }

        public IImmutableList<RestoreCall> RestoreCalls { get; private set; } = ImmutableList<RestoreCall>.Empty;
        public class RestoreCall
        {
            public RestoreCall(
                string gangId)
            {
                GangId = gangId;
            }

            public string GangId { get; }
        }
    }
}
