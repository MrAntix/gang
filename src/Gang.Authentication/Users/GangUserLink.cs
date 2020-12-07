namespace Gang.Authentication.Users
{
    public sealed class GangUserLink
    {
        public GangUserLink(
            string name, string emailAddress,
            GangUserLinkCode oneTimeCode,
            object data = null
            )
        {
            Name = name;
            EmailAddress = emailAddress;
            OneTimeCode = oneTimeCode;
            Data = data;
        }

        public string Name { get; }
        public string EmailAddress { get; }
        public GangUserLinkCode OneTimeCode { get; }
        public object Data { get; }
    }
}
