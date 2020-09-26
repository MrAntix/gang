using Gang.Web.Services.State;

namespace Gang.Web.Services.Events
{
    public class WebGangUserNameUpdatedEvent :
        WebGangStateEvent, IWebGangUserChangeName
    {
        public WebGangUserNameUpdatedEvent(
            string userId,
            string name,
            GangMessageAudit audit) : base(audit)
        {
            UserId = userId;
            Name = name;
        }

        public string UserId { get; }
        public string Name { get; }
    }
}
