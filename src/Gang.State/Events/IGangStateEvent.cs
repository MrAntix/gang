namespace Gang.State.Events
{
    public interface IGangStateEvent
    {
        object Data { get; }
        GangAudit Audit { get; }
    }
}
