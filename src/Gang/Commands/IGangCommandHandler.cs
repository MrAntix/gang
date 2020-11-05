using Gang.Contracts;
using System.Threading.Tasks;

namespace Gang.Commands
{
    public interface IGangCommandHandler<THost, TCommand>
        where THost : GangHostBase
    {
        Task HandleAsync(THost host, TCommand command, GangAudit audit);
    }
}