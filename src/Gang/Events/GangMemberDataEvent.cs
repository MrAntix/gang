namespace Gang.Events
{
    public class GangMemberDataEvent :
        GangMemberEvent
    {
        public GangMemberDataEvent(
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
