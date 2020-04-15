using Gang.Events;
using System;
using System.Threading.Tasks;

namespace Gang
{
    public abstract class GangEventHandlerBase<TEvent> : IGangEventHandler
        where TEvent : GangEvent
    {
        public Type EventType { get; } = typeof(TEvent);

        Task IGangEventHandler.HandleAsync(GangEvent e)
        {
            return HandleAsync((TEvent)e);
        }

        public abstract Task HandleAsync(TEvent e);
    }
}
