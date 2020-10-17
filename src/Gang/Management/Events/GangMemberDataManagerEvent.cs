namespace Gang.Management.Events
{
    public class GangMemberDataManagerEvent :
        GangMemberManagerEvent
    {
        public GangMemberDataManagerEvent(
            string gangId,
            IGangMember member,
            byte[] data
            ) : base(gangId, member)
        {
            Data = data;
        }

        public byte[] Data { get; }
    }
}
