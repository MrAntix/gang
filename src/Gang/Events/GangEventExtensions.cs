using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gang.Events
{
    public static class GangEventExtensions
    {
        public static IImmutableList<GangStateEventWrapper> Add(
            this IEnumerable<GangStateEventWrapper> wrappers,
            object @event, GangStateEventAudit audit
            )
        {
            return wrappers
                .ToImmutableList()
                .Add(new GangStateEventWrapper(@event, audit));
        }
    }
}
