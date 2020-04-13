namespace Gang.Events
{
    public class GangMemberEvent :
        GangEvent
    {
        public GangMemberEvent(
            string gangId,
            IGangMember member
            ) : base(gangId)
        {
            Member = member;
        }

        public IGangMember Member { get; }
    }
}
