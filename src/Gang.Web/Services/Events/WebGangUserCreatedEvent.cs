namespace Gang.Web.Services.Events
{
    public class WebGangUserCreatedEvent 
    {
        public WebGangUserCreatedEvent(
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
