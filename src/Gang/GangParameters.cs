namespace Gang
{
    public class GangParameters
    {
        public GangParameters(
            string gangId,
            string token = null)
        {
            GangId = gangId;
            Token = token;
        }

        public string GangId { get; }
        public string Token { get; }
    }
}
