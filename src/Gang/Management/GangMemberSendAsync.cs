using Gang.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gang.Members
{
    public delegate Task GangMemberSendAsync(
        byte[] data,
        GangMessageTypes? type = null,
        IEnumerable<byte[]> memberIds = null);
}
