using Gang.Events;
using System;
using System.Threading.Tasks;

namespace Gang
{
    public interface IGangEventHandler
    {
        Type EventType { get; }
        Task HandleAsync(GangEvent e);
    }
}
