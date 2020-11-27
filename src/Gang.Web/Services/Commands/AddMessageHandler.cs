using Gang.State.Commands;
using Gang.Web.Services.Commands;
using Gang.Web.Services.State;
using System.Threading.Tasks;

namespace Gang.Web.Services
{
    public class AddMessageHandler :
        IGangCommandHandler<AddMessage, WebGangAggregate>
    {
        async Task<WebGangAggregate> IGangCommandHandler<AddMessage, WebGangAggregate>
             .HandleAsync(
                 WebGangAggregate aggregate,
                 GangCommand<AddMessage> command)
        {
            return aggregate.AddMessage(
                command.Data.Id, command.Data.Text,
                command.Audit.UserId
                );
        }
    }
}
