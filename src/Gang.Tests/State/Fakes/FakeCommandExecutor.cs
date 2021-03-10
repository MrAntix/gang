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

        public IGangCommand Deserialize(byte[] bytes, GangAudit audit)
        {
            return GangCommand.From(bytes, audit);
        }

        public Task<GangState<TodosState>> ExecuteAsync(
            GangState<TodosState> state, IGangCommand command)
        {
            ExecuteCalls = ExecuteCalls
                .Add(new ExecuteCall(
                    state, command
                ));

            return Task.FromResult(
                _getResult(state)
                );
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
            public ExecuteCall(GangState<TodosState> state, IGangCommand command)
            {
                State = state;
                Command = command;
            }

            public GangState<TodosState> State { get; }
            public IGangCommand Command { get; }
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
