using Gang.Contracts;

namespace Gang.Events
{
    public sealed class GangEventWrapper
    {
        public GangEventWrapper(
            object @event, GangMessageAudit audit)
        {
            Event = @event;
            Audit = audit;
        }

        public object Event { get; }
        public GangMessageAudit Audit { get; }
    }
}
