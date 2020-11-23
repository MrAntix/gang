namespace Gang.Management.Events
{
    public sealed class GangManagerEvent<TEventData> :
        IGangManagerEvent
    {
        public GangManagerEvent(
            TEventData data,
            GangAudit audit
        )
        {
            Data = data;
            Audit = audit;
        }

        public TEventData Data { get; }
        public GangAudit Audit { get; }

        object IGangManagerEvent.Data => Data;
    }

}
