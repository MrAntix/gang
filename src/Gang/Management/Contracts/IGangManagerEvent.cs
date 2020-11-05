using Gang.Contracts;

namespace Gang.Management.Contracts
{
    public interface IGangManagerEvent
    {
        object Data { get; }
        GangAudit Audit { get; }
    }

}
