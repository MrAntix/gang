namespace Gang.Events
{
    public sealed class GangStateEventWrapper
    {
        public GangStateEventWrapper(
            object @event, GangStateEventAudit audit)
        {
            Event = @event;
            Audit = audit;
        }

        public object Event { get; }
        public GangStateEventAudit Audit { get; }
    }
}
