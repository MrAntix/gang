using System;
using System.Collections.Immutable;

namespace Gang
{
    public class GangCollection
    {
        static readonly object lockObject = new object();

        IImmutableDictionary<string, GangMemberCollection> _gangs;

        public GangCollection()
        {
            _gangs = ImmutableDictionary<string, GangMemberCollection>.Empty;
        }

        public bool ContainsGang(string gangId)
        {
            return _gangs.ContainsKey(gangId);
        }

        public GangMemberCollection this[string gangId]
        {
            get
            {
                _gangs.TryGetValue(gangId, out var gang);
                return gang;
            }
        }

        public bool TryAddGang(
            string gangId,
            out GangMemberCollection gang)
        {
            lock (lockObject)
            {
                if (ContainsGang(gangId))
                {
                    gang = _gangs[gangId];
                    return false;
                }

                gang = new GangMemberCollection();
                _gangs = _gangs.Add(gangId, gang);

                return true;
            }
        }

        public GangMemberCollection AddMemberToGang(
            string gangId, IGangMember member)
        {
            lock (lockObject)
            {
                var gang = _gangs[gangId].AddMember(member);

                _gangs = _gangs.SetItem(gangId, gang);

                return gang;
            }
        }

        public void RemoveMemberFromGang(
            string gangId, IGangMember member)
        {
            var gang = _gangs[gangId].RemoveMember(member);

            if (gang == null)
            {
                _gangs = _gangs.Remove(gangId);
            }
            else
            {
                _gangs = _gangs.SetItem(gangId, gang);
            }
        }
    }
}
