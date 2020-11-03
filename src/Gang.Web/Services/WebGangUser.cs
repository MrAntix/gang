namespace Gang.Web.Services
{
    public sealed class WebGangUser
    {
        public WebGangUser(
            string id,
            string token)
        {
            Id = id;
            Token = token;
        }

        public string Id { get; }
        public string Token { get; }
    }
}
