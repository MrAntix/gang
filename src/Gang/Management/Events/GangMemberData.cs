namespace Gang.Management.Events
{
    public class GangMemberData
    {
        public GangMemberData(
            byte[] data
            )
        {
            Data = data;
        }

        public byte[] Data { get; }
    }
}
