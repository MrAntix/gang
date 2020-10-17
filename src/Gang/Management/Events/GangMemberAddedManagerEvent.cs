namespace Gang.Management.Events
{
    public class GangMemberAddedManagerEvent :
        GangMemberManagerEvent
    {
        public GangMemberAddedManagerEvent(
            string gangId,
            IGangMember member
            ) : base(gangId, member)
        {
        }
    }
}
