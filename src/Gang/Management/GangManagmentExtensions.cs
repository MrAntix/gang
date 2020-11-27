using System.Threading.Tasks;

namespace Gang.Management
{
    public static class GangManagmentExtensions
    {
        public static IGangMember GetMember(
            this IGangController controller,
            byte[] memberId
            )
        {
            return controller.GetGang().MemberById(memberId);
        }

        public static Task DisconnectAsync(
            this IGangController controller,
            string memberId,
            string reason = null)
        {
            return controller.DisconnectAsync(memberId.GangToBytes(), reason);
        }
    }
}
