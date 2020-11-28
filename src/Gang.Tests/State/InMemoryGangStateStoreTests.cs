using Gang.State;
using Gang.State.Storage;
using Gang.Tests.State.Todos;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Gang.Tests.State
{
    public sealed class InMemoryGangStateStoreTests
    {
        const string GANG_ID = "GANG_ID";
        const string TODO_ID = "TODO_ID";
        readonly GangState<TodosState> STATE
            = new GangState<TodosState>()
                .AddTodo(TODO_ID)
                .CompleteTodo(TODO_ID, DateTimeOffset.Now);
        readonly GangAudit AUDIT = new(GANG_ID);

        [Fact]
        public async Task subscribe_latest_default()
        {
            var store = GetStore();
            await store.CommitAsync(GANG_ID, STATE, AUDIT);

            var sn = 0U;
            var count = 0;
            using var s1 = store.Subscribe(e =>
            {
                sn = e.Sequence;
                count++;
                return Task.CompletedTask;
            });

            Assert.Equal(0U, sn);
            Assert.Equal(0, count);
        }

        [Fact]
        public async Task subscribe_start()
        {
            var store = GetStore();
            await store.CommitAsync(GANG_ID, STATE, AUDIT);

            var sn = 0U;
            var count = 0;
            using var s1 = store.Subscribe(e =>
            {
                sn = e.Sequence;
                count++;
                return Task.CompletedTask;
            }, 0);

            Assert.Equal(2U, sn);
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task subscribe_skip()
        {
            var store = GetStore();
            await store.CommitAsync(GANG_ID, STATE, AUDIT);

            var sn = 0U;
            var count = 0;
            using var s1 = store.Subscribe(e =>
            {
                sn = e.Sequence;
                count++;
                return Task.CompletedTask;
            }, 1);

            Assert.Equal(2U, sn);
            Assert.Equal(1, count);
        }

        static IGangStateStore GetStore()
        {
            return new InMemoryGangStateStore();
        }
    }
}
