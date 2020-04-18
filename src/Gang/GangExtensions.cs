using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gang
{
    public static class GangExtensions
    {
        public static IGangMember MemberById(
            this GangMemberCollection gang,
            string id)
        {
            return gang.MemberById(
                Encoding.UTF8.GetBytes(id)
                );
        }

        public static T TryGetById<T>(
            this IEnumerable<T> list,
            byte[] id)
            where T : IHasGangId
        {
            return list.FirstOrDefault(item => item.Id.SequenceEqual(id));
        }
    }
}
