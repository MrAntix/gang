namespace Gang.Management.Events
{
    public sealed class GangMemberData
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
