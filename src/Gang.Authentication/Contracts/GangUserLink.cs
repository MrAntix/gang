namespace Gang.Authentication.Contracts
{
    public class GangUserLink
    {
        public GangUserLink(
            string name, string emailAddress,
            GangUserToken token,
            object data = null
            )
        {
            Name = name;
            EmailAddress = emailAddress;
            Token = token;
            Data = data;
        }

        public string Name { get; }
        public string EmailAddress { get; }
        public GangUserToken Token { get; }
        public object Data { get; }
    }
}
