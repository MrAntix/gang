namespace Gang.Events
{
    public class GangMemberRemovedEvent :
        GangMemberEvent
    {
        public GangMemberRemovedEvent(
            string gangId,
            IGangMember member
            ) : base(gangId, member)
        {
        }
    }
}
