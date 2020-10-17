using Gang.Contracts;
using System.Threading.Tasks;

namespace Gang.Events
{
    public interface IGangEventHandler<THost, TEvent>
        where THost : GangHostBase
    {
        Task HandleAsync(THost host, TEvent e, GangMessageAudit audit);
    }
}
