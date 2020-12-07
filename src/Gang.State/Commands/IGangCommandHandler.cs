using System.Threading.Tasks;

namespace Gang.State.Commands
{
    public interface IGangCommandHandler<TStateData, TCommandData>
        where TStateData : class
        where TCommandData : class
    {
        Task<GangState<TStateData>> HandleAsync(
            GangState<TStateData> state, GangCommand<TCommandData> command);
    }
}