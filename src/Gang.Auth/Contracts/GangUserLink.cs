namespace Gang.Auth.Contracts
{
    public class GangUserLink
    {
        public GangUserLink(
            string name, string emailAddress,
            GangUserToken token
            )
        {
            Name = name;
            EmailAddress = emailAddress;
            Token = token;
        }

        public string Name { get; }
        public string EmailAddress { get; }
        public GangUserToken Token { get; }
    }
}
