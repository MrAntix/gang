using Gang.State;
using Gang.State.Events;
using Gang.Tests.State.Todos;
using Gang.Tests.State.Todos.Add;
using Gang.Tests.State.Todos.Complete;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Gang.Tests.State
{
    public class GangStateTests
    {
        const string TODO_ID = "TODO_ID";
        const string GANG_ID = "GANG_ID";

        [Fact]
        public async Task generates_events()
        {
            var state = new GangState<TodosState>()
                .AddTodo(TODO_ID)
                .CompleteTodo(TODO_ID, DateTimeOffset.Now);

            Assert.Equal(2, state.Uncommitted.Count);
            Assert.IsType<TodoAdded>(state.Uncommitted[0]);
            Assert.IsType<TodoCompleted>(state.Uncommitted[1]);
        }

        [Fact]
        public async Task uncommitted_as_sequence()
        {
            var state = new GangState<TodosState>()
                .AddTodo(TODO_ID)
                .CompleteTodo(TODO_ID, DateTimeOffset.Now);

            var events = GangEvent.SequenceFrom(
                state.Uncommitted,
                new GangAudit(GANG_ID, 10)
                );

            Assert.Equal(2, events.Count);
            Assert.Equal(11U, events[0].Audit.SequenceNumber);
            Assert.Equal(12U, events[1].Audit.SequenceNumber);
        }

        [Fact]
        public async Task apply_events()
        {
            var events = GangEvent.SequenceFrom(
                    new object[]
                    {
                        new TodoAdded(TODO_ID),
                        new TodoCompleted(TODO_ID, DateTimeOffset.Now)
                    },
                    new GangAudit(GANG_ID)
                );

            var state = new GangState<TodosState>()
                .Apply(events);

            var todo = Assert.Single(state.Data.Todos.Values);

            Assert.Equal(TODO_ID, todo.Id);
            Assert.NotNull(todo.CompletedOn);
        }
    }
}
