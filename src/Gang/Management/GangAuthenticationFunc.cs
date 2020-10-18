using Gang.Contracts;
using System.Threading.Tasks;

namespace Gang.Management
{
    public delegate Task<GangAuth> GangAuthenticationFunc(GangParameters parameters);
}
