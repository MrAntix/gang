using System;
using System.Threading.Tasks;

namespace Gang.State.Commands
{
    public interface IGangCommandExecutor<TStateData>
        where TStateData : class
    {
        IGangCommand Deserialize(
            byte[] bytes, GangAudit audit
            );

        Task<GangState<TStateData>> ExecuteAsync(GangState<TStateData> state, IGangCommand command);

        IGangCommandExecutor<TStateData> RegisterHandler<TCommandData>(GangCommandHandler<TStateData> handler)
            where TCommandData : class;
        IGangCommandExecutor<TStateData> RegisterHandlerProvider<TCommandData>(Func<IGangCommandHandler<TStateData, TCommandData>> provider)
            where TCommandData : class;
    }
}