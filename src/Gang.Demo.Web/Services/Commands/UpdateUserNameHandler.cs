using Gang.Demo.Web.Services.State;
using Gang.State;
using Gang.State.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace Gang.Demo.Web.Services.Commands
{
    public sealed class UpdateUserNameHandler :
        IGangCommandHandler<HostState, UpdateUserName>
    {
        Task<GangState<HostState>> IGangCommandHandler<HostState, UpdateUserName>
            .HandleAsync(GangState<HostState> state, GangCommand<UpdateUserName> command)
        {
            var user = state.Data.Users.TryGetById(command.Audit.UserId);
            var otherUserIds = state.Data.Users.Where(u => u != user).Select(u => u.Id).ToArray();

            if (user == null)
            {
                return Task.FromResult(
                    state
                        .CreateUser(
                            command.Audit.UserId, command.Data.Name
                        )
                        .AddUserMessage(
                            $"Hello @{command.Audit.UserId}, welcome to the gang",
                            command.Audit.UserId
                        )
                        .AddUserMessage(
                            $"@{command.Audit.UserId} joined the gang",
                            otherUserIds
                        )
                    );
            }

            return Task.FromResult(
                user.Name == command.Data.Name
                ? state
                : state
                    .SetUserName(
                        command.Audit.UserId, command.Data.Name
                    )
                    .AddUserMessage(
                        $"{user.Name} changed their name to @{user.Id}",
                        otherUserIds
                    )
                );
        }
    }
}
