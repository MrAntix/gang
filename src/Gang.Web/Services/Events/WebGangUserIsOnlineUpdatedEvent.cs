using Gang.Web.Services.State;

namespace Gang.Web.Services.Events
{
    public class WebGangUserIsOnlineUpdatedEvent :
        WebGangStateEvent, IWebGangUserChangeIsOnline
    {
        public WebGangUserIsOnlineUpdatedEvent(
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
