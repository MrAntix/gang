using Gang.State;
using Gang.Tests.State.Todos;
using Gang.Tests.State.Todos.Add;
using Gang.Tests.State.Todos.Complete;
using System;
using Xunit;

namespace Gang.Tests.State
{
    public sealed class GangStateTests
    {
        const string TODO_ID = "TODO_ID";
        const string GANG_ID = "GANG_ID";

        [Fact]
        public void generates_events()
        {
            var state = GangState.Create(TodosState.Initial)
                .AddTodo(TODO_ID)
                .CompleteTodo(TODO_ID, DateTimeOffset.Now);

            Assert.Equal(2, state.Uncommitted.Count);
            Assert.IsType<TodoAdded>(state.Uncommitted[0]);
            Assert.IsType<TodoCompleted>(state.Uncommitted[1]);
        }

        [Fact]
        public void uncommitted_as_sequence()
        {
            var state = GangState.Create(TodosState.Initial)
                .AddTodo(TODO_ID)
                .CompleteTodo(TODO_ID, DateTimeOffset.Now);

            var events = GangState<TodosState>.EventSequenceFrom(
                state.Uncommitted,
                new GangAudit(GANG_ID, 10)
                );

            Assert.Equal(2, events.Count);
            Assert.Equal(11U, events[0].Audit.Version);
            Assert.Equal(12U, events[1].Audit.Version);
        }

        [Fact]
        public void apply_events()
        {
            var events = GangState<TodosState>.EventSequenceFrom(
                    new object[]
                    {
                        new TodoAdded(TODO_ID),
                        new TodoCompleted(TODO_ID, DateTimeOffset.Now)
                    },
                    new GangAudit(GANG_ID)
                );

            var state = GangState.Create(TodosState.Initial)
                .Apply(events);

            var todo = Assert.Single(state.Data.Todos.Values);

            Assert.Equal(TODO_ID, todo.Id);
            Assert.NotNull(todo.CompletedOn);
        }
    }
}
