namespace Gang.Management.Contracts
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
