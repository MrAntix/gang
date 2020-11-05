using Gang.Commands;
using Gang.Contracts;
using Gang.Web.Services.Commands;
using System.Threading.Tasks;

namespace Gang.Web.Services
{
    public class AddMessageCommandHandler :
        IGangCommandHandler<WebGangHost, AddMessageCommand>
    {
        async Task IGangCommandHandler<WebGangHost, AddMessageCommand>.HandleAsync(WebGangHost host, AddMessageCommand command, GangAudit audit)
        {
            await host.AddMessage(
                command.Text,
                command.Id,
                audit
             );

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
