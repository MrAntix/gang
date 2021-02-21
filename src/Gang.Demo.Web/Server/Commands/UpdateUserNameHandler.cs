using Gang.Demo.Web.Server.State;
using Gang.State;
using Gang.State.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace Gang.Demo.Web.Server.Commands
{
    public sealed class UpdateUserNameHandler :
        IGangCommandHandler<HostState, UpdateUserName>
    {
        Task<GangState<HostState>> IGangCommandHandler<HostState, UpdateUserName>
            .HandleAsync(GangState<HostState> state, GangCommand<UpdateUserName> command)
        {
            var user = state.Data.Users.TryGetById(command.Audit.UserId);
            var otherUserIds = state.Data.Users.Where(u => u != user).Select(u => u.UserId).ToArray();

            if (user == null)
            {
                state = state
                        .CreateUser(
                            command.Audit.UserId, command.Data.Name
                        )
                        .AddUserMessage(
                            "Welcome",
                            $"Hello @{command.Audit.UserId}, welcome to the gang",
                            new[] { command.Audit.UserId }
                        )
                        .AddUserMessage(
                            $"@{command.Audit.UserId} joined the gang",
                            otherUserIds
                    );
            }
            else
            {
                state =
                    user.Name == command.Data.Name
                    ? state
                    : state
                        .SetUserName(
                            command.Audit.UserId, command.Data.Name
                        )
                        .AddUserMessage(
                            $"{user.Name} changed their name to @{user.UserId}",
                            otherUserIds
                    );
            }

            //if (!state.HasErrors)
            //{
            //    var gangUser = await _userStore.TryGetByIdAsync(command.Audit.UserId);
            //    await _userStore.SetAsync(gangUser.SetName(command.Data.Name));
            //}

            return Task.FromResult(state);
        }
    }
}
