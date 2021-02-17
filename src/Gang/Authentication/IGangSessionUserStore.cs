using System.Threading.Tasks;

namespace Gang.Authentication
{
    public interface IGangSessionUserStore
    {
        Task<GangSessionUser> TryGetByIdAsync(string id);
        Task SetAsync(GangSessionUser value);
    }
}
