namespace Gang.Events
{
    public abstract class GangEvent
    {
        public GangEvent(
            string gangId
            )
        {
            GangId = gangId;
        }

        public string GangId { get; }
    }
}
