namespace Gang.Contracts
{
    public sealed class GangEvent : IGangEvent
    {
        public GangEvent(
            object data, GangAudit audit)
        {
            Data = data;
            Audit = audit;
        }

        public object Data { get; }
        public GangAudit Audit { get; }
    }
}
