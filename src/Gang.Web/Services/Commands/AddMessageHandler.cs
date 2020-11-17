using Antix.Handlers;
using Gang.Commands;
using Gang.Web.Services.Commands;
using System.Threading.Tasks;

namespace Gang.Web.Services
{
    public class AddMessageHandler :
        IHandler<GangCommand<AddMessage>, WebGangHost>
    {
        async Task IHandler<GangCommand<AddMessage>, WebGangHost>
             .HandleAsync(GangCommand<AddMessage> command, WebGangHost host)
        {
            await host.AddMessage(
                command.Data.Text,
                command.Data.Id,
                command.Audit
             );

            await host.NotifyAsync(
                new Notify(
                    "success", null
                ),
                new[] { command.Audit.MemberId },
                command.Audit.SequenceNumber
            );
        }
    }
}
