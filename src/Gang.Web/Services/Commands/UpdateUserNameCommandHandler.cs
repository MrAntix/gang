using Gang.Commands;
using Gang.Contracts;
using Gang.Web.Services.Commands;
using System.Threading.Tasks;

namespace Gang.Web.Services
{
    public class UpdateUserNameCommandHandler :
        IGangCommandHandler<WebGangHost, UpdateUserNameCommand>
    {
        async Task IGangCommandHandler<WebGangHost, UpdateUserNameCommand>
            .HandleAsync(WebGangHost host, UpdateUserNameCommand command, GangMessageAudit audit)
        {
            await host.UpdateUser(
                command.Id, command.Name,
                audit);

            await host.NotifyAsync(
                new NotifyCommand(
                    "success", null
                ),
                new[] { audit.MemberId },
                audit.SequenceNumber
            );
        }
    }
}
