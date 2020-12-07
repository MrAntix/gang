namespace Gang
{
    public sealed class GangAuth
    {
        public GangAuth(
            byte[] memberId,
            string token,
            GangApplication application)
        {
            MemberId = memberId;
            Token = token;
            Application = application;
        }

        public byte[] MemberId { get; }
        public string Token { get; }
        public GangApplication Application { get; }
    }
}
