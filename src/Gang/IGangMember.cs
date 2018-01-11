using System.Threading.Tasks;

namespace Gang
{
    public interface IGangMember
    {
        byte[] Id { get; }

        bool IsConnected { get; }
        Task DisconnectAsync(string reason = "disconnected");

        Task SendAsync(GangMessageTypes type, byte[] message);
        Task<byte[]> ReceiveAsync();
    }
}
