using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Gang.Web.Services.State
{
    public class WebGangUser : IHasGangIdString
    {
        public WebGangUser(
            string id,
            string name,
            IEnumerable<string> memberIds,
            IEnumerable<string> properties
            )
        {
            Id = id;
            Name = name;
            MemberIds = memberIds?.ToImmutableList();
            Properties = properties?.ToImmutableList();
            IsOnline = MemberIds?.Any() ?? false;
        }

        public string Id { get; }
        public string Name { get; }
        public IImmutableList<string> MemberIds { get; }
        public IEnumerable<string> Properties { get; }
        public bool IsOnline { get; }

        public WebGangUser Update(IWebGangUserChangeName change)
        {
            return new WebGangUser(
                Id,
                change.Name,
                MemberIds, Properties
                );
        }

        public WebGangUser Update(IWebGangUserMemberIdsUpdatedEvent change)
        {
            return new WebGangUser(
                Id,
                Name,
                change.MemberIds, Properties
                );
        }
    }
}
