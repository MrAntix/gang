using Gang.Commands;
using Gang.Web.Services.Commands;
using Gang.Web.Services.Events;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gang.Web.Services
{
    public class UpdateUserNameCommandHandler :
        GangCommandHandlerBase<WebGangHost, UpdateUserNameCommand>
    {
        public override string CommandTypeName { get; } = "updateUserName";

        protected override async Task HandleAsync(
            WebGangHost host, UpdateUserNameCommand command, GangMessageAudit audit)
        {
            var user = host.State.Users.First(u => u.Id == command.Id);
            var joined = user.Name == null;

            var e = new WebGangUserNameUpdatedEvent(
                user.Id,
                command.Name
            );

            await host.RaiseStateEventAsync(e, audit.MemberId, host.State.Apply);

            if (joined)
                await host.SendWelcome(Encoding.UTF8.GetBytes(user.Id));
        }
    }
}
