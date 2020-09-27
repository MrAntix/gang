using Gang.Web.Services.State;

namespace Gang.Web.Services.Events
{
    public class WebGangUserNameUpdatedEvent :
        IWebGangUserChangeName
    {
        public WebGangUserNameUpdatedEvent(
            string userId,
            string name)
        {
            UserId = userId;
            Name = name;
        }

        public string UserId { get; }
        public string Name { get; }
    }
}
