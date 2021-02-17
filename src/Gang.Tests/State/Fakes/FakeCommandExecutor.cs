using Gang.State;
using Gang.State.Commands;
using Gang.Tests.State.Todos;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Gang.Tests.State.Fakes
{
    public sealed class FakeCommandExecutor :
        IGangCommandExecutor<TodosState>
    {
        readonly Func<GangState<TodosState>, GangState<TodosState>> _getResult;

        public FakeCommandExecutor(
            Func<GangState<TodosState>, GangState<TodosState>> getResult = null)
        {
            _getResult = getResult
                ?? (state => state);
        }

        public Task<GangState<TodosState>> ExecuteAsync(
            GangState<TodosState> state, byte[] bytes, GangAudit audit)
        {
            ExecuteCalls = ExecuteCalls
                .Add(new ExecuteCall(
                    state, bytes, audit
                ));

            return Task.FromResult(
                _getResult(state)
                );
        }

        public Task<GangState<TodosState>> ExecuteAsync<TCommand>(GangState<TodosState> state, TCommand command, GangAudit audit)
        {
            throw new NotImplementedException();
        }

        public FakeCommandExecutor WithError()
        {
            return new FakeCommandExecutor(
                a => a.RaiseErrors("ERROR")
                );
        }

        public IImmutableList<ExecuteCall> ExecuteCalls { get; private set; } = ImmutableList<ExecuteCall>.Empty;
        public sealed class ExecuteCall
        {
            public ExecuteCall(GangState<TodosState> state, byte[] bytes, GangAudit audit)
            {
                State = state;
                Bytes = bytes;
                Audit = audit;
            }

            public GangState<TodosState> State { get; }
            public byte[] Bytes { get; }
            public GangAudit Audit { get; }
        }

        public IGangCommandExecutor<TodosState> RegisterHandler<TCommandData>(GangCommandHandler<TodosState> handler)
            where TCommandData : class
        {
            throw new NotImplementedException();
        }

        public IGangCommandExecutor<TodosState> RegisterHandlerProvider<TCommandData>(Func<IGangCommandHandler<TodosState, TCommandData>> provider)
            where TCommandData : class
        {
            throw new NotImplementedException();
        }
    }
}
