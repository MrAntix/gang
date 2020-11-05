namespace Gang.Contracts
{
    public sealed class GangEvent
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
