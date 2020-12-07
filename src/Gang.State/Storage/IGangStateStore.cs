using Gang.State.Events;
using System;
using System.Threading.Tasks;

namespace Gang.State.Storage
{
    public interface IGangStateStore
    {
        Task<GangState<TStateData>> RestoreAsync<TStateData>(string gangId, TStateData initial)
            where TStateData : class;
        Task<GangState<TStateData>> CommitAsync<TStateData>(string gangId, GangState<TStateData> state, GangAudit audit)
            where TStateData : class;
        IDisposable Subscribe(Func<IGangStateEvent, Task> observer, uint? startSequenceNumber = null);
    }
}
