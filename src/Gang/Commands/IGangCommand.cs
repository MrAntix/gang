using Gang.Contracts;

namespace Gang.Commands
{
    public interface IGangCommand
    {
        object Data { get; }
        GangAudit Audit { get; }
    }
}