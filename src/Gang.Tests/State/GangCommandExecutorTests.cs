using Gang.Commands;
using Gang.Serialization;
using Gang.State;
using Gang.State.Commands;
using Gang.Tests.State.Fakes;
using Gang.Tests.State.Todos;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Gang.Tests.State
{
    public sealed class GangCommandExecutorTests
    {
        const string GANG_ID = "GANG_ID";
        const string TYPE_NAME = "object";
        readonly object DATA = new();
        readonly GangAudit AUDIT = new(GANG_ID);

        [Fact]
        public async Task throws_when_no_handler()
        {
            var serialization = GetSerializationService()
                .SetupDeserialize(new GangCommandWrapper(TYPE_NAME, DATA));
            var executor = GetExecutor(serialization: serialization);
            var state = new GangState<TodosState>();

            await Assert.ThrowsAsync<GangCommandHandlerNotFoundExcetion>(
                async () => await executor.ExecuteAsync(state, null, AUDIT)
                );
        }

        [Fact]
        public async Task throws_when_handler_throws()
        {
            var serialization = GetSerializationService()
                .SetupDeserialize(new GangCommandWrapper(TYPE_NAME, DATA));
            var handler = new FakeHandler((s, c) => throw new Exception());
            var executor = GetExecutor(serialization: serialization)
                .RegisterHandlerProvider(() => handler);
            var state = new GangState<TodosState>();

            await Assert.ThrowsAsync<Exception>(
                async () => await executor.ExecuteAsync(state, null, AUDIT)
                );
        }

        [Fact]
        public async Task handle_gets_command()
        {
            var serialization = GetSerializationService()
                .SetupDeserialize(new GangCommandWrapper(TYPE_NAME, DATA));
            var handler = new FakeHandler();
            var executor = GetExecutor(serialization: serialization)
                .RegisterHandlerProvider(() => handler);
            var state = new GangState<TodosState>();

            await executor.ExecuteAsync(state, null, AUDIT);

            var call = Assert.Single(handler.HandleCalls);
            Assert.IsType<GangCommand<object>>(call.Command);
        }

        static FakeSerializationService GetSerializationService()
        {
            return new FakeSerializationService();
        }

        static IGangCommandExecutor<TodosState> GetExecutor(
            IGangSerializationService serialization = null)
        {
            return new GangCommandExecutor<TodosState>(
                serialization ?? GetSerializationService()
                );
        }
    }
}
