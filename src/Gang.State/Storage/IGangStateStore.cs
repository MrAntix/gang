using Gang.State.Events;
using System;
using System.Threading.Tasks;

namespace Gang.State.Storage
{
    public interface IGangStateStore
    {
        Task<GangState<TStateData>> RestoreAsync<TStateData>(string gangId)
            where TStateData : class, new();
        Task<GangState<TStateData>> CommitAsync<TStateData>(string gangId, GangState<TStateData> state, GangAudit audit)
            where TStateData : class, new();
        IDisposable Subscribe(Func<IGangEvent, Task> observer, uint? startSequence = null);
    }
}
