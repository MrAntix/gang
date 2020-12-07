using System.Threading.Tasks;

namespace Gang.Authentication
{
    public delegate Task<GangSession> GangAuthenticationFunc(GangParameters parameters);
}
