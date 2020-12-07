namespace Gang.State.Storage
{
    public sealed class GangStateEventWrapper
    {
        public GangStateEventWrapper(
            object data,
            string type,
            GangAudit audit
            )
        {
            Data = data;
            Type = type;
            Audit = audit;
        }

        public object Data { get; }
        public string Type { get; }
        public GangAudit Audit { get; }
    }
}
