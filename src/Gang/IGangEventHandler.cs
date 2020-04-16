using Gang.Events;
using System.Threading.Tasks;

namespace Gang
{
    public interface IGangEventHandler
    {
        Task HandleAsync(GangEvent e);
    }
}
