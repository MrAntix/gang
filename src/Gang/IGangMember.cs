using System.Threading.Tasks;

namespace Gang
{
    public interface IGangMember
    {
        byte[] Id { get; }
        bool IsOpen { get; }

        Task SendAsync(GangMessageTypes type, byte[] message);
        Task<byte[]> ReceiveAsync();
    }
}
