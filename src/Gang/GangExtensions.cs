using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gang
{
    public static class GangExtensions
    {
        public static IGangMember MemberById(
            this GangMemberCollection gang,
            string id)
        {
            return gang.MemberById(
                GangToBytes(id)
                );
        }

        public static T TryGetById<T>(
            this IEnumerable<T> list,
            byte[] id)
            where T : IHasGangId
        {
            return list.FirstOrDefault(item => item.Id.SequenceEqual(id));
        }

        public static string GangToString(
            this byte[] value)
        {
            return Encoding.UTF8.GetString(value);
        }

        public static byte[] GangToBytes(
            this string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        public static Task SendCommandAsync<T>(
            this IGangController controller,
            string type, T command,
            IEnumerable<byte[]> memberIds = null)
        {
            return controller.SendCommandAsync<T>(
                new GangCommandWrapper(type, command),
                memberIds);
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
