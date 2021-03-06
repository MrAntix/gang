using Gang.State.Commands;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gang.State
{
    public class GangStateNotification
    {
        public GangStateNotification(
            IEnumerable<byte[]> memberIds,
            GangNotify command
            )
        {
            MemberIds = memberIds.ToImmutableListDefaultEmpty();
            Command = command;
        }

        public IImmutableList<byte[]> MemberIds { get; }
        public GangNotify Command { get; }
    }
}