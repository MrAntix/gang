using Gang.Commands;
using Gang.Management;
using Gang.Serialization;
using Gang.State;
using Gang.State.Commands;
using Gang.State.Storage;
using Gang.Tests.Management.Fakes;
using Gang.Tests.State.Fakes;
using Gang.Tests.State.Todos;
using Gang.Tests.State.Todos.Complete;
using Gang.WebSockets.Serialization;
using System.Threading.Tasks;
using Xunit;

namespace Gang.Tests.State
{
    public class GangStateHostTests
    {
        const string TODO_ID = "TODO_ID";
        const string GANG_ID = "GANG_ID";
        readonly GangParameters _parameters = new(GANG_ID);

        [Fact]
        public async Task restores_state_on_connect()
        {
            var store = GetStore();
            await GetManagedHostAsync(store: store);

            var call = Assert.Single(store.RestoreCalls);
            Assert.NotNull(call.GangId);
        }

        [Fact]
        public async Task commit_on_command()
        {
            var store = GetStore();
            var host = await GetManagedHostAsync(store: store);

            await HandleCommandAsync(host, new CompleteTodo(TODO_ID));

            var call = Assert.Single(store.CommitCalls);
            Assert.NotNull(call.GangId);
            Assert.NotNull(call.State);
            Assert.NotNull(call.Audit);
        }

        [Fact]
        public async Task no_commit_on_command_error()
        {
            var executor = GetExecutor().WithError();
            var store = GetStore();
            var host = await GetManagedHostAsync(executor: executor, store: store);

            await HandleCommandAsync(host, new CompleteTodo(TODO_ID));

            Assert.Empty(store.CommitCalls);
        }

        [Fact]
        public async Task execute_on_command()
        {
            var executor = GetExecutor();
            var host = await GetManagedHostAsync(executor: executor);

            await HandleCommandAsync(host, new CompleteTodo(TODO_ID));

            var call = Assert.Single(executor.ExecuteCalls);
            Assert.NotNull(call.State);
            Assert.NotNull(call.Bytes);
            Assert.NotNull(call.Audit);
        }

        static readonly IGangSerializationService _serialization = new WebSocketGangJsonSerializationService();

        static Task HandleCommandAsync<TCommandData>(
            IGangMember member, TCommandData data,
            string userId = null)
        {
            return member.HandleAsync(GangMessageTypes.Command,
                _serialization.SerializeCommandData(data),
                new GangAudit(GANG_ID, null, null, userId)
                );
        }

        static FakeCommandExecutor GetExecutor()
        {
            return new FakeCommandExecutor();
        }

        static FakeStateStore GetStore()
        {
            return new FakeStateStore();
        }

        async Task<GangStateHost<TodosState>> GetManagedHostAsync(
            IGangCommandExecutor<TodosState> executor = null,
            IGangStateStore<TodosState> store = null)
        {
            var host = new GangStateHost<TodosState>(
                executor ?? GetExecutor(),
                store ?? GetStore()
                );

            await new FakeGangManager()
                .ManageAsync(_parameters, host);

            return host;
        }
    }
}
