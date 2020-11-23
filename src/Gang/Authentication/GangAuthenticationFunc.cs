using System.Threading.Tasks;

namespace Gang.Authentication
{
    public delegate Task<GangAuth> GangAuthenticationFunc(GangParameters parameters);
}
