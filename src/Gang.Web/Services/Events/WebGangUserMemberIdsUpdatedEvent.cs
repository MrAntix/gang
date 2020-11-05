using Gang.Web.Services.State;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gang.Web.Services.Events
{
    public class WebGangUserMemberIdsUpdatedEvent :
        IWebGangUserMemberIdsUpdatedEvent
    {
        public WebGangUserMemberIdsUpdatedEvent(
            string userId,
            IEnumerable<string> memberIds
            )
        {
            UserId = userId;
            MemberIds = memberIds?.ToImmutableList()
                ?? ImmutableList<string>.Empty;
        }

        public string UserId { get; }
        public IImmutableList<string> MemberIds { get; }
    }
}
