using Gang.State.Commands;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gang.State
{
    public class GangStateNotification
    {
        public GangStateNotification(
            IEnumerable<string> userIds,
            GangNotify command
            )
        {
            UserIds = userIds.ToImmutableListDefaultEmpty();
            Command = command;
        }

        public IImmutableList<string> UserIds { get; }
        public GangNotify Command { get; }
    }
}