namespace Gang.Management.Events
{
    public class GangRemovedManagerEvent :
        GangManagerEvent
    {
        public GangRemovedManagerEvent(
            string gangId
            ) : base(gangId)
        {
        }
    }
}
