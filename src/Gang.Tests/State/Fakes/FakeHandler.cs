using Gang.State;
using Gang.State.Commands;
using Gang.Tests.State.Todos;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Gang.Tests.State.Fakes
{
    public sealed class FakeHandler :
        IGangCommandHandler<TodosState, object>
    {
        readonly Func<GangState<TodosState>, GangCommand<object>, GangState<TodosState>> _getResult;

        public FakeHandler(
            Func<GangState<TodosState>, GangCommand<object>, GangState<TodosState>> getResult = null)
        {
            _getResult = getResult
                ?? ((GangState<TodosState> r, GangCommand<object> _) => r);
        }

        Task<GangState<TodosState>> IGangCommandHandler<TodosState, object>
            .HandleAsync(GangState<TodosState> state, GangCommand<object> command)
        {
            HandleCalls = HandleCalls.Add(new HandleCall(state, command));

            return Task.FromResult(_getResult(state, command));
        }

        public IImmutableList<HandleCall> HandleCalls { get; private set; } = ImmutableList<HandleCall>.Empty;
        public sealed class HandleCall
        {
            public HandleCall(
                GangState<TodosState> state, GangCommand<object> command)
            {
                State = state;
                Command = command;
            }

            public GangState<TodosState> State { get; }
            public GangCommand<object> Command { get; }
        }
    }
}
