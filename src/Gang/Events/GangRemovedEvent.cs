namespace Gang.Events
{
    public class GangRemovedEvent :
        GangEvent
    {
        public GangRemovedEvent(
            string gangId
            ) : base(gangId)
        {
        }
    }
}
