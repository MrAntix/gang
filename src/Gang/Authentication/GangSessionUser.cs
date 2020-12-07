namespace Gang.Authentication
{
    public sealed class GangSessionUser
    {
        public GangSessionUser(
            string id,
            string name = null, string emailAddress = null)
        {
            Id = id;
            Name = name;
            EmailAddress = emailAddress;
        }

        public string Id { get; }
        public string Name { get; }
        public string EmailAddress { get; }
    }
}
