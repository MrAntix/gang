namespace Gang.Events
{
    public interface IGangEvent
    {
        object Data { get; }
        GangAudit Audit { get; }
    }
}
