namespace Gang.Web.Services.Events
{
    public class WebGangUserCreated
    {
        public WebGangUserCreated(
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
