namespace Gang.Web.Services.State
{

    public class WebGangUser : IHasGangIdString
    {
        public WebGangUser(
            string id,
            string name
            )
        {
            Id = id;
            Name = name;
        }

        public string Id { get; }
        public string Name { get; }

        internal WebGangUser SetName(
            string name)
        {
            return new WebGangUser(
                Id,
                name
                );
        }
    }
}
