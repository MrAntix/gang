using Gang.State.Commands;
using Gang.Web.Services.Commands;
using Gang.Web.Services.State;
using System.Threading.Tasks;

namespace Gang.Web.Services
{
    public sealed class UpdateUserNameHandler :
        IGangCommandHandler<UpdateUserName, WebGangAggregate>
    {
        async Task<WebGangAggregate> IGangCommandHandler<UpdateUserName, WebGangAggregate>
            .HandleAsync(
                WebGangAggregate aggregate,
                GangCommand<UpdateUserName> command)
        {
            return aggregate.SetUserName(
                command.Audit.UserId, command.Data.Name
                );
        }
    }
}
