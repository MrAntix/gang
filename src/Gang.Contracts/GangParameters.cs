namespace Gang.Contracts
{
    public class GangParameters
    {
        public GangParameters(
            string gangId)
        {
            GangId = gangId;
        }

        public string GangId { get; }
    }
}
