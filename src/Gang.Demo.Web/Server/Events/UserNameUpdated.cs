namespace Gang.Demo.Web.Server.Events
{
    public sealed class UserNameUpdated
    {
        public UserNameUpdated(
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
