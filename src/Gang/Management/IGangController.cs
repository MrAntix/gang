using Gang.Contracts;
using Gang.Members;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gang.Management
{
    public interface IGangController
    {
        string GangId { get; }
        GangMemberCollection GetGang();
        Task SendAsync(byte[] data, GangMessageTypes? type = null, IEnumerable<byte[]> memberIds = null);
        Task SendCommandAsync(string type, object command, IEnumerable<byte[]> memberIds = null, uint? inReplyToSequenceNumber = null);
        Task SendStateAsync<T>(T state, IEnumerable<byte[]> memberIds = null);
        Task DisconnectAsync(byte[] memberId, string reason);
    }
}