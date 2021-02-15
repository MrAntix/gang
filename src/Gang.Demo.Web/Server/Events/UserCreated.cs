namespace Gang.Demo.Web.Server.Events
{
    public sealed class UserCreated
    {
        public UserCreated(
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
