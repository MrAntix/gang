using System.Threading.Tasks;

namespace Gang.Management.Events
{
    public abstract class GangManagerEventHandlerBase<TEvent> : IGangManagerEventHandler
        where TEvent : GangManagerEvent
    {
        protected abstract Task HandleAsync(TEvent e);

        Task IGangManagerEventHandler.HandleAsync(GangManagerEvent e)
        {
            return HandleAsync((TEvent)e);
        }
    }
}
