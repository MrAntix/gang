namespace Gang.Web.Services.Events
{
    public class WebGangUserCreatedEvent 
    {
        public WebGangUserCreatedEvent(
            string userId,
            string name
            )
        {
            UserId = userId;
            Name = name;
        }

        public string UserId { get; }
        public string Name { get; }
    }
}
