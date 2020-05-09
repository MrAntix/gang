using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gang
{
    public interface IGangController
    {
        string GangId { get; }
        GangMemberCollection GetGang();
        Task SendAsync(byte[] data, GangMessageTypes? type = null, IEnumerable<byte[]> memberIds = null);
        Task SendCommandAsync(IGangCommandWrapper wrapper, IEnumerable<byte[]> memberIds = null);
        Task SendStateAsync<T>(T state, IEnumerable<byte[]> memberIds = null);
        Task DisconnectAsync(byte[] memberId, string reason);
    }
}