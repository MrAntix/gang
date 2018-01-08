using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Gang
{
    public class Gang
    {
        public Gang(
            IGangMember host,
            IEnumerable<IGangMember> members = null)
        {
            Host = host;
            Members = members == null
                ? ImmutableArray<IGangMember>.Empty
                : members.ToImmutableArray();
        }

        public IGangMember Host { get; }
        public IImmutableList<IGangMember> Members { get; }

        public Gang AddMember(IGangMember member)
        {

            return new Gang(Host, Members.Add(member));
        }

        public Gang RemoveMember(IGangMember member)
        {
            if (member == Host)
            {
                return Members.Any()
                    ? new Gang(Members[0], Members.Skip(1))
                    : null;
            }

            return new Gang(Host, Members.Remove(member));
        }

    }
}
