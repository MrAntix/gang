using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gang.Web.Services.Events
{
    public class WebGangUserCreated
    {
        public WebGangUserCreated(
            string userId,
            string name,
            byte[] memberId,
            IEnumerable<string> properties
            )
        {
            UserId = userId;
            Name = name;
            MemberId = memberId;
            Properties = properties?.ToImmutableList();
        }

        public string UserId { get; }
        public string Name { get; }
        public byte[] MemberId { get; }
        public IImmutableList<string> Properties { get; }
    }
}
