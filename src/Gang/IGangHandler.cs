using Gang.Contracts;
using System.Threading.Tasks;

namespace Gang
{
    public interface IGangHandler
    {
        Task HandleAsync(GangParameters parameters, IGangMember gangMember);
    }
}
