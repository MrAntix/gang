namespace Gang.Events
{
    public class GangAddedEvent :
        GangEvent
    {
        public GangAddedEvent(
            string gangId
            ) : base(gangId)
        {
        }
    }
}
