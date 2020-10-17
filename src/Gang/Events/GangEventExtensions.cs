using Gang.Contracts;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gang.Events
{
    public static class GangEventExtensions
    {
        public static IImmutableList<GangEventWrapper> Add(
            this IEnumerable<GangEventWrapper> wrappers,
            object @event, GangMessageAudit audit
            )
        {
            return wrappers
                .ToImmutableList()
                .Add(new GangEventWrapper(@event, audit));
        }
    }
}
