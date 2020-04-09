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
    }
}
