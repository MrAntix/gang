using Gang.Web.Services.State;

namespace Gang.Web.Services.Events
{
    public sealed class WebGangUserNameUpdated :
        IWebGangUserChangeName
    {
        public WebGangUserNameUpdated(
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
