namespace Gang.Contracts
{
    public class GangParameters
    {
        public GangParameters(
            string gangId,
            string token)
        {
            GangId = gangId;
            Token = token;
        }

        public string GangId { get; }
        public string Token { get; }
    }
}
