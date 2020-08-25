using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gang
{
    public static class GangExtensions
    {
        public async static Task BlockAsync(
            this Task<GangMemberConnectionState> task)
        {
            var state = await task;

            await state.BlockingTask;
        }

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

        public static T TryGetByIdString<T>(
            this IEnumerable<T> list,
            byte[] id)
            where T : IHasGangIdString
        {
            var idString = id.GangToString();
            return list.FirstOrDefault(item => item.Id == idString);
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
            T command,
            IEnumerable<byte[]> memberIds = null)
        {
            return controller.SendCommandAsync(
                GetCommandType(command), command,
                memberIds);
        }

        public static Task DisconnectAsync(
            this IGangController controller,
            string memberId,
            string reason = null)
        {
            return controller.DisconnectAsync(memberId.GangToBytes(), reason);
        }

        public static string GetCommandType(
            object command)
        {
            var name = command.GetType().Name;
            return name.EndsWith("Command")
                ? name.Substring(0, name.Length - 7)
                : name;
        }
    }
}
