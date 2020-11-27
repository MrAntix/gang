using System;
using System.Threading.Tasks;

namespace Gang.State.Commands
{
    public interface IGangCommandExecutor<TStateData>
        where TStateData : class, new()
    {
        Task<GangState<TStateData>> ExecuteAsync(GangState<TStateData> state, byte[] bytes, GangAudit audit);
        IGangCommandExecutor<TStateData> RegisterHandler<TCommandData>(GangCommandHandler<TStateData> handler);
        IGangCommandExecutor<TStateData> RegisterHandlerProvider<TCommandData>(Func<IGangCommandHandler<TStateData, TCommandData>> provider);
    }
}