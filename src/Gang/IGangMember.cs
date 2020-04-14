using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gang
{
    public interface IGangMember : IDisposable
    {
        byte[] Id { get; }

        Task ConnectAsync(string gangId, GangMemberSendAsync sendAsync);
        Task DisconnectAsync(string reason = "disconnected");

        Task SendAsync(GangMessageTypes type, byte[] data, byte[] memberId = null);
    }

    public delegate Task GangMemberSendAsync(
        byte[] data,
        GangMessageTypes? type = null,
        IEnumerable<byte[]> messageIds = null);
}
