using Gang.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gang.Management
{
    public interface IGangController
    {
        string GangId { get; }
        GangMemberCollection GetGang();
        Task ReceiveAsync(byte[] data);
        Task SendAsync(GangMessageTypes? type, byte[] data, IEnumerable<byte[]> memberIds = null);
        Task SendCommandAsync(string type, object data, IEnumerable<byte[]> memberIds = null, uint? inReplyToSequenceNumber = null);
        Task SendStateAsync<T>(T state, IEnumerable<byte[]> memberIds = null);
        Task DisconnectAsync(byte[] memberId, string reason);
    }
}