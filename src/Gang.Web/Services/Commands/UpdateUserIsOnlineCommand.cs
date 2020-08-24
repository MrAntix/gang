using Gang.Web.Services.State;

namespace Gang.Web.Services.Commands
{
    public class UpdateUserIsOnlineCommand :
        IWebGangUserChangeIsOnline
    {
        public UpdateUserIsOnlineCommand(
            string id,
            bool isOnline)
        {
            Id = id;
            IsOnline = isOnline;
        }

        public string Id { get; }
        public bool IsOnline { get; }
    }
}
