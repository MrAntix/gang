using System.Collections.Immutable;

namespace Gang
{
    public class GangCollection
    {
        static readonly object lockObject = new object();
        IImmutableDictionary<string, Gang> _gangs;

        public GangCollection()
        {
            _gangs = ImmutableDictionary<string, Gang>.Empty;
        }

        public Gang this[string gangId]
        {
            get
            {
                _gangs.TryGetValue(gangId, out var gang);
                return gang;
            }
        }

        public void AddMemberToGang(
            string gangId, IGangMember member)
        {
            lock (lockObject)
            {
                Gang gang;
                if (_gangs.ContainsKey(gangId))
                {
                    gang = _gangs[gangId].AddMember(member);
                    _gangs = _gangs.SetItem(gangId, gang);
                }
                else
                {
                    gang = new Gang(member);
                    _gangs = _gangs.Add(gangId, gang);
                }
            }
        }

        public void RemoveMemberFromGang(
            string gangId, IGangMember member)
        {
            var gang = _gangs[gangId].RemoveMember(member);

            if (gang == null)
                _gangs = _gangs.Remove(gangId);

            else
                _gangs = _gangs.SetItem(gangId, gang);
        }
    }
}
