using Gang.Events;
using System.Threading.Tasks;

namespace Gang
{
    public abstract class GangEventHandlerBase<TEvent> : IGangEventHandler
        where TEvent : GangEvent
    {
        protected abstract Task HandleAsync(TEvent e);

        Task IGangEventHandler.HandleAsync(GangEvent e)
        {
            return HandleAsync((TEvent)e);
        }
    }
}
