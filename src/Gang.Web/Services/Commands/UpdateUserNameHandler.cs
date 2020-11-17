using Antix.Handlers;
using Gang.Commands;
using Gang.Web.Services.Commands;
using System.Threading.Tasks;

namespace Gang.Web.Services
{
    public class UpdateUserNameHandler :
        IHandler<GangCommand<UpdateUserName>, WebGangHost>
    {
        async Task IHandler<GangCommand<UpdateUserName>, WebGangHost>
              .HandleAsync(GangCommand<UpdateUserName> command, WebGangHost host)
        {
            await host.UpdateUser(
                command.Data.Name,
                command.Audit);

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
