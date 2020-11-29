using Gang.Demo.Web.Services.State;
using Gang.State;
using Gang.State.Commands;
using System.Threading.Tasks;

namespace Gang.Demo.Web.Services.Commands
{
    public sealed class AddMessageHandler :
        IGangCommandHandler<HostState, AddMessage>
    {
        Task<GangState<HostState>> IGangCommandHandler<HostState, AddMessage>
            .HandleAsync(GangState<HostState> state, GangCommand<AddMessage> command)
        {
            return Task.FromResult(
                state.AddMessage(
                    command.Data.Id, command.Data.Text,
                    command.Audit.UserId, command.Audit.On
                    )
                );
        }
    }
}
