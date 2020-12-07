namespace Gang.Authentication.Users
{
    public sealed class GangUserLink
    {
        public GangUserLink(
            string name, string emailAddress,
            GangUserLinkCode code,
            object data = null
            )
        {
            Name = name;
            EmailAddress = emailAddress;
            Code = code;
            Data = data;
        }

        public string Name { get; }
        public string EmailAddress { get; }
        public GangUserLinkCode Code { get; }
        public object Data { get; }
    }
}
