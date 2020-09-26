namespace Gang.Web.Services.Events
{
    public class WebGangUserCreatedEvent :
        WebGangStateEvent
    {
        public WebGangUserCreatedEvent(
            string userId,
            bool isOnline,
            GangMessageAudit audit) : base(audit)
        {
            UserId = userId;
            IsOnline = isOnline;
        }

        public string UserId { get; }
        public bool IsOnline { get; }
    }
}
