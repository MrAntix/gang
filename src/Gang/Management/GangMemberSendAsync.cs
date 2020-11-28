using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gang.Management
{
    public delegate Task GangMemberSendAsync(
        GangMessageTypes? type,
        byte[] data,
        GangAudit audit,
        IEnumerable<byte[]> memberIds = null);
}
