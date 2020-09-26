namespace Gang.Web.Services.Commands
{
    public class UpdateUserIsOnlineCommand
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
