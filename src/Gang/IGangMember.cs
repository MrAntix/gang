using System;
using System.Threading.Tasks;

namespace Gang
{
    public interface IGangMember : IDisposable
    {
        byte[] Id { get; }

        Task ConnectAsync(IGangController controller, Func<Task> onDisconnectAsync);
        Task DisconnectAsync(string reason = "disconnected");

        Task SendAsync(GangMessageTypes type, byte[] data, byte[] memberId = null);
    }
}
