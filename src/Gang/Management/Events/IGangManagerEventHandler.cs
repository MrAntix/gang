using System.Threading.Tasks;

namespace Gang.Management.Events
{
    public interface IGangManagerEventHandler
    {
        Task HandleAsync(GangManagerEvent e);
    }
}
