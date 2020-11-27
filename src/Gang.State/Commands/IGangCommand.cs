namespace Gang.State.Commands
{
    public interface IGangCommand
    {
        object Data { get; }
        GangAudit Audit { get; }
    }
}