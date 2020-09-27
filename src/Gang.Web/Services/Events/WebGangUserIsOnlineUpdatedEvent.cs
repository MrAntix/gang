using Gang.Web.Services.State;

namespace Gang.Web.Services.Events
{
    public class WebGangUserIsOnlineUpdatedEvent :
        IWebGangUserChangeIsOnline
    {
        public WebGangUserIsOnlineUpdatedEvent(
            string userId,
            bool isOnline
            )
        {
            UserId = userId;
            IsOnline = isOnline;
        }

        public string UserId { get; }
        public bool IsOnline { get; }
    }
}
