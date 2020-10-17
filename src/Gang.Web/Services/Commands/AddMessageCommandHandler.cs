using Gang.Commands;
using Gang.Contracts;
using Gang.Web.Services.Commands;
using Gang.Web.Services.Events;
using System.Threading.Tasks;

namespace Gang.Web.Services
{
    public class AddMessageCommandHandler :
        IGangCommandHandler<WebGangHost, AddMessageCommand>
    {
        async Task IGangCommandHandler<WebGangHost, AddMessageCommand>.HandleAsync(WebGangHost host, AddMessageCommand command, GangMessageAudit audit)
        {
            var e = new WebGangMessageAddedEvent(
                 command.Id,
                 command.Text
             );

            await host.RaiseStateEventAsync(e, audit.MemberId, host.State.Apply);

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
