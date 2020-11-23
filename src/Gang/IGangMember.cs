using Gang.Authentication;
using Gang.Management;
using System;
using System.Threading.Tasks;

namespace Gang
{
    public interface IGangMember : IDisposable
    {
        byte[] Id { get; }
        GangAuth Auth { get; }

        Task ConnectAsync(IGangController controller, Func<Task> onDisconnectAsync);
        Task DisconnectAsync(string reason = "disconnected");

        Task HandleAsync(GangMessageTypes type, byte[] data, GangAudit audit = null);
    }
}
