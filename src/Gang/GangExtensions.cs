using System.Text;

namespace Gang
{
    public static class GangExtensions
    {
        public static IGangMember MemberById(
            this Gang gang,
            string id)
        {
            return gang.MemberById(
                Encoding.UTF8.GetBytes(id)
                );
        }
    }
}
