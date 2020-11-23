using Gang.Management;
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

        public static T TryGetById<T>(
            this IEnumerable<T> list,
            string id)
            where T : IHasGangIdString
        {
            return list.FirstOrDefault(item => item.Id == id);
        }

        public static IEnumerable<T> TryRemoveById<T>(
            this IEnumerable<T> list,
            byte[] id)
            where T : IHasGangId
        {
            return list.Where(item => !item.Id.SequenceEqual(id));
        }

        public static IEnumerable<T> TryRemoveById<T>(
            this IEnumerable<T> list,
            string id)
            where T : IHasGangIdString
        {
            return list.Where(item => item.Id != id);
        }

        public static string GangToString(
            this byte[] value)
        {
            if (value == null) return null;

            return Encoding.UTF8.GetString(value);
        }

        public static byte[] GangToBytes(
            this string value)
        {
            if (value == null) return null;

            return Encoding.UTF8.GetBytes(value);
        }

        public static string TryDecapitalize(
            this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;

            var chars = value.ToCharArray();

            chars[0] = char.ToLower(chars[0]);

            return new string(chars);
        }

        public static string TryTrimEnd(
            this string value, string end)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;

            var trimBy = value
                .EndsWith(end, StringComparison.InvariantCultureIgnoreCase)
                ? end.Length
                : 0;

            return value.Substring(0, value.Length - trimBy);
        }
    }
}
