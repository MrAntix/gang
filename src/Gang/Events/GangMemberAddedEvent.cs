namespace Gang.Events
{
    public class GangMemberAddedEvent :
        GangMemberEvent
    {
        public GangMemberAddedEvent(
            string gangId,
            IGangMember member
            ) : base(gangId, member)
        {
        }
    }
}
