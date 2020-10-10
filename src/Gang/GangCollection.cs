using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gang
{
    public class GangCollection : IEnumerable<GangMemberCollection>
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

        public GangMemberCollection AddMemberToGang(
            string gangId, IGangMember member,
            Action<GangMemberCollection> onNewGang = null)
        {
            lock (lockObject)
            {
                var gang = this[gangId];
                if (gang == null)
                {
                    gang = new GangMemberCollection();
                    _gangs = _gangs.Add(gangId, gang);

                    onNewGang?.Invoke(gang);
                }

                gang = this[gangId]
                    .AddMember(member);

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

        IEnumerator<GangMemberCollection> IEnumerable<GangMemberCollection>.GetEnumerator() => _gangs.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _gangs.Values.GetEnumerator();
    }
}
