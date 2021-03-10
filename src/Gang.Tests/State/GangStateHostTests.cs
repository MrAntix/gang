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
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Gang.Tests.State
{
    public sealed class GangStateHostTests
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
            Assert.NotNull(call.Command);
        }

        [Fact]
        public async Task receipt_on_command()
        {
            var manager = GetManager();
            var host = await GetManagedHostAsync(manager: manager);
            var sequence = 99U;

            await HandleCommandAsync(
                host,
                new CompleteTodo(TODO_ID),
                sequence: sequence
                );

            var args = Assert.Single(manager.Sent);
            Assert.Equal(GangMessageTypes.Receipt, args.Type);
            Assert.Equal(sequence, BitConverter.ToUInt32(args.Data.ToArray()));
        }

        static readonly IGangSerializationService _serialization = new WebSocketGangJsonSerializationService();

        static Task HandleCommandAsync<TCommandData>(
            IGangMember member, TCommandData data,
            string userId = null,
            uint sequence = 1)
        {
            return member.HandleAsync(GangMessageTypes.Command,
                _serialization.SerializeCommandData(data),
                new GangAudit(GANG_ID, sequence, null, userId)
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

        static FakeGangManager GetManager()
        {
            return new FakeGangManager();
        }

        async Task<GangStateHost<TodosState>> GetManagedHostAsync(
            IGangCommandExecutor<TodosState> executor = null,
            IGangStateStore store = null,
            IGangManager manager = null)
        {
            var host = new GangStateHost<TodosState>(
                executor ?? GetExecutor(),
                store ?? GetStore(),
                new TodosState()
                );

            manager ??= GetManager();

            await manager
                .ManageAsync(_parameters, host);

            return host;
        }
    }
}
