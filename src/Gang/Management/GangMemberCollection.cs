using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Gang.Members
{
    public class GangMemberCollection
    {
        public GangMemberCollection(
            IGangMember hostMember = null,
            IEnumerable<IGangMember> otherMembers = null)
        {
            HostMember = hostMember;
            OtherMembers = otherMembers == null
                ? ImmutableArray<IGangMember>.Empty
                : otherMembers.ToImmutableArray();
            Members = HostMember == null
                ? OtherMembers
                : OtherMembers.Add(HostMember);
        }

        public IGangMember HostMember { get; }
        public IImmutableList<IGangMember> OtherMembers { get; }
        public IImmutableList<IGangMember> Members { get; }

        public IGangMember MemberById(byte[] id)
        {
            return Members.Single(m => m.Id.SequenceEqual(id));
        }

        public GangMemberCollection AddMember(IGangMember member)
        {
            if (HostMember == null)
                return new GangMemberCollection(member);

            return new GangMemberCollection(HostMember, OtherMembers.Add(member));
        }

        public GangMemberCollection RemoveMember(IGangMember member)
        {
            if (member == HostMember)
                return OtherMembers.Any()
                    ? new GangMemberCollection(OtherMembers[0], OtherMembers.Skip(1))
                    : null;

            return new GangMemberCollection(HostMember, OtherMembers.Remove(member));
        }
    }
}
