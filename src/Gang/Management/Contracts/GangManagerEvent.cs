using Gang.Contracts;

namespace Gang.Management.Contracts
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
