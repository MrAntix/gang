using System;

namespace Gang.Web.Services.State
{
    public class WebGangUser
    {
        public WebGangUser(
            string id,
            string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; }
        public string Name { get; }

        public WebGangUser Update(IWebGangUserChange change)
        {
            return new WebGangUser(
                Id,
                change.Name
                );
        }
    }
}
