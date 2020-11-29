namespace Gang.Management.Events
{
    public interface IGangManagerEvent
    {
        object Data { get; }
        GangAudit Audit { get; }
    }
}
