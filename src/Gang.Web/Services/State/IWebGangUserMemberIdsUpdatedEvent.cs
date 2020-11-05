using System.Collections.Immutable;

namespace Gang.Web.Services.State
{
    public interface IWebGangUserMemberIdsUpdatedEvent
    {
        IImmutableList<string> MemberIds { get; }
    }
}
