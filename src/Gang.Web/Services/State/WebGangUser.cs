namespace Gang.Web.Services.State
{
    public class WebGangUser : IHasGangIdString
    {
        public WebGangUser(
            string id,
            string name,
            bool isOnline)
        {
            Id = id;
            Name = name;
            IsOnline = isOnline;
        }

        public string Id { get; }
        public string Name { get; }
        public bool IsOnline { get; }

        public WebGangUser Update(IWebGangUserChangeName change)
        {
            return new WebGangUser(
                Id,
                change.Name,
                IsOnline
                );
        }

        public WebGangUser Update(IWebGangUserChangeIsOnline change)
        {
            return new WebGangUser(
                Id,
                Name,
                change.IsOnline
                );
        }
    }
}
