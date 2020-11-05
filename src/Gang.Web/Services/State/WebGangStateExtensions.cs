using System.Collections.Generic;
using System.Linq;

namespace Gang.Web.Services.State
{
    public static class WebGangStateExtensions
    {
        public static WebGangUser TryGetByMemberId(
            this IEnumerable<WebGangUser> users,
            byte[] id)
        {
            if (users is null)
                throw new System.ArgumentNullException(nameof(users));
            if (id is null)
                throw new System.ArgumentNullException(nameof(id));

            var idString = id.GangToString();

            return users.FirstOrDefault(u =>
                u.MemberIds?.Any(mId => mId == idString) ?? false
            );
        }
    }
}
