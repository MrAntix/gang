using Gang.State;
using Gang.State.Commands;
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
        const string MEMBER_ID = "MEMBER_ID";

        readonly GangAudit AUDIT = new(GANG_ID, memberId: "AUDIT_MEMBER_ID".GangToBytes());

        const string ERROR_TEXT = "ERROR_TEXT";
        const string NOTIFY_TEXT = "NOTIFY_TEXT";

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
        public void assert_fail_raises_error_state_not_changed()
        {
            var state = GangState.Create(TodosState.Initial)
                .AddTodo(TODO_ID)
                .Assert(false, ERROR_TEXT)
                .CompleteTodo(TODO_ID, DateTimeOffset.Now);

            Assert.Equal(1, state.Uncommitted.Count);
            Assert.IsType<TodoAdded>(state.Uncommitted[0]);
            Assert.Equal(1, state.Errors.Count);
            Assert.Equal(ERROR_TEXT, state.Errors[0]);
        }

        [Fact]
        public void get_results()
        {
            var state = GangState.Create(TodosState.Initial)
                .AddResult(MEMBER_ID.GangToBytes(), new GangNotify(NOTIFY_TEXT));

            var results = state.GetResults(AUDIT);

            Assert.Equal(1, results.Count);

            var result = Assert.IsType<GangStateResult<GangNotify>>(results[0]);

            Assert.Equal(MEMBER_ID.GangToBytes(), Assert.Single(result.SendToMemberIds));
            Assert.Equal(NOTIFY_TEXT, result.Command.Id);
        }

        [Fact]
        public void get_result_when_error_no_notification()
        {
            var state = GangState.Create(TodosState.Initial)
                .AddResult(MEMBER_ID.GangToBytes(), new GangNotify(NOTIFY_TEXT))
                .Assert(false, ERROR_TEXT);

            var results = state.GetResults(AUDIT);

            Assert.Equal(1, results.Count);

            var result = Assert.IsType<GangStateResult<GangNotify>>(results[0]);

            Assert.Equal(AUDIT.MemberId, Assert.Single(result.SendToMemberIds));
            Assert.Equal(ERROR_TEXT, result.Command.Id);
            Assert.Equal(GangNotificationTypes.Danger, result.Command.Type);
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
