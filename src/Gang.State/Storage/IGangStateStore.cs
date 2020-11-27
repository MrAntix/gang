using System.Threading.Tasks;

namespace Gang.State.Storage
{
    public interface IGangStateStore<TStateData>
        where TStateData : class, new()
    {
        Task<GangState<TStateData>> RestoreAsync(string gangId);
        Task CommitAsync(string gangId, GangState<TStateData> state, GangAudit audit);
    }
}
