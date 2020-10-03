using Gang.Commands;
using Gang.Web.Services.Commands;
using Gang.Web.Services.Events;
using System.Threading.Tasks;

namespace Gang.Web.Services
{
    public class AddMessageCommandHandler :
        GangCommandHandlerBase<WebGangHost, AddMessageCommand>
    {
        public override string CommandTypeName { get; } = "addMessage";

        protected override async Task HandleAsync(
            WebGangHost host, AddMessageCommand command, GangMessageAudit audit)
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
