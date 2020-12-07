namespace Gang.Authentication.Users
{
    public sealed class GangUserChallenge
    {
        public GangUserChallenge(
            GangApplication application,
            string userId,
            string name, string emailAddress,
            string challenge)
        {
            Application = application;
            UserId = userId;
            Name = name;
            EmailAddress = emailAddress;
            Challenge = challenge;
        }

        public GangApplication Application { get; }
        public string UserId { get; }
        public string Name { get; }
        public string EmailAddress { get; }
        public string Challenge { get; }
    }
}
