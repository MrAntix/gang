using System.Threading.Tasks;

namespace Gang.State
{
    public interface IGangStateCache
    {
        Task PutAsync<TState>(string gangId, GangState<TState> wrapper);
        Task<GangState<TState>> GetAsync<TState>(string gangId);
    }
}
