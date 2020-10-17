namespace Gang.Management.Events
{
    public class GangAddedManagerEvent :
        GangManagerEvent
    {
        public GangAddedManagerEvent(
            string gangId
            ) : base(gangId)
        {
        }
    }
}
