namespace Gang.Management.Events
{
    public abstract class GangManagerEvent
    {
        public GangManagerEvent(
            string gangId
            )
        {
            GangId = gangId;
        }

        public string GangId { get; }
    }
}
