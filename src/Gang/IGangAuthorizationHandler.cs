using Gang.Contracts;
using System.Threading.Tasks;

namespace Gang
{
    public interface IGangAuthorizationHandler
    {
        Task<bool> AuthorizeAsync(GangParameters parameters);
    }
}
