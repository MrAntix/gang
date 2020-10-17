namespace Gang.Management.Events
{
    public class GangMemberRemovedManagerEvent :
        GangMemberManagerEvent
    {
        public GangMemberRemovedManagerEvent(
            string gangId,
            IGangMember member
            ) : base(gangId, member)
        {
        }
    }
}
