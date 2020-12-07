namespace Gang.Authentication.Users
{
    public sealed class GangUserLink
    {
        public GangUserLink(
            string name, string email,
            GangUserLinkCode code,
            object data = null
            )
        {
            Name = name;
            Email = email;
            Code = code;
            Data = data;
        }

        public string Name { get; }
        public string Email { get; }
        public GangUserLinkCode Code { get; }
        public object Data { get; }
    }
}
