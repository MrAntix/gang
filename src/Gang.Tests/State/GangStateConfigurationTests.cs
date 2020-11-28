using Gang.Commands;
using Gang.Serialization;
using Gang.State;
using Gang.State.Commands;
using Gang.Tests.State.Todos;
using Gang.Tests.State.Todos.Add;
using Gang.WebSockets;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Gang.Tests.State
{
    public sealed class GangStateConfigurationTests
    {
        const string GANG_ID = "GANG_ID";
        const string USER_ID = "USER_ID";
        const string TODO_ID = "TODO_ID";
        readonly GangAudit AUDIT = new(GANG_ID, userId: USER_ID);

        [Fact]
        public void register_command_services()
        {
            var services = new ServiceCollection()
                .AddGangState<TodosState>();

            Assert.Equal(1, services.Count(r => r.ServiceType == typeof(IGangCommandExecutor<TodosState>)));
            // note: finds FakeHandler too
            Assert.Equal(3, services.Count(r => r.ServiceType == typeof(GangCommandHandler<TodosState>)));
        }

        [Fact]
        public async Task execute_a_command()
        {
            var sp = new ServiceCollection()
                .AddWebSocketGangsSerialization()
                .AddGangState<TodosState>()
                .BuildServiceProvider();

            var serialization = sp.GetRequiredService<IGangSerializationService>();
            var commandHandler = sp.GetRequiredService<IGangCommandExecutor<TodosState>>();
            var data = serialization.SerializeCommandData(new AddTodo(TODO_ID));

            var result = await commandHandler.ExecuteAsync(
                new GangState<TodosState>(),
                data,
                AUDIT
                );

            Assert.Single(result.Data.Todos);
        }
    }
}
