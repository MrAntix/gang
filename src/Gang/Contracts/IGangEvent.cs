namespace Gang.Contracts
{
    public interface IGangEvent
    {
        object Data { get; }
        GangAudit Audit { get; }
    }

}
