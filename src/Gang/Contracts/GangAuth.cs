namespace Gang.Contracts
{
    public class GangAuth
    {
        public GangAuth(
            byte[] memberId,
            byte[] token)
        {
            MemberId = memberId;
            Token = token;
        }

        public byte[] MemberId { get; }
        public byte[] Token { get; }
    }
}
