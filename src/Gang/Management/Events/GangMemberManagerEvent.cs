namespace Gang.Management.Events
{
    public class GangMemberManagerEvent :
        GangManagerEvent
    {
        public GangMemberManagerEvent(
            string gangId,
            IGangMember member
            ) : base(gangId)
        {
            Member = member;
        }

        public IGangMember Member { get; }
    }
}
