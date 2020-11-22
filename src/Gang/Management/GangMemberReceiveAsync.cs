using System.Threading.Tasks;

namespace Gang.Management
{
    public delegate Task GangMemberReceiveAsync(
        byte[] data);
}
