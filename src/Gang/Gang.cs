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
            HostMember = host;
            OtherMembers = members == null
                ? ImmutableArray<IGangMember>.Empty
                : members.ToImmutableArray();
        }

        public IGangMember HostMember { get; }
        public IImmutableList<IGangMember> OtherMembers { get; }

        public IGangMember MemberById(byte[] id)
        {
            return HostMember.Id.SequenceEqual(id)
                ? HostMember
                : OtherMembers.Single(m => m.Id.SequenceEqual(id));
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
