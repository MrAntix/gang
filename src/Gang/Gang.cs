using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Gang
{
    public class Gang
    {
        public Gang(
            IGangMember hostMember,
            IEnumerable<IGangMember> otherMembers = null)
        {
            HostMember = hostMember;
            OtherMembers = otherMembers == null
                ? ImmutableArray<IGangMember>.Empty
                : otherMembers.ToImmutableArray();
            Members=OtherMembers.Add(HostMember);
        }

        public IGangMember HostMember { get; }
        public IImmutableList<IGangMember> OtherMembers { get; }
        public IImmutableList<IGangMember> Members { get;}

        public IGangMember MemberById(byte[] id)
        {
            return Members.Single(m => m.Id.SequenceEqual(id));
        }

        public Gang AddMember(IGangMember member)
        {
            return new Gang(HostMember, OtherMembers.Add(member));
        }

        public Gang RemoveMember(IGangMember member)
        {
            if (member == HostMember)
            {
                return OtherMembers.Any()
                    ? new Gang(OtherMembers[0], OtherMembers.Skip(1))
                    : null;
            }

            return new Gang(HostMember, OtherMembers.Remove(member));
        }
    }
}
