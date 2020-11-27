using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Gang.State.Events
{
    public static class GangEvent
    {
        public static IGangEvent From(object data, GangAudit audit)
        {
            var EventType = typeof(GangEvent<>).MakeGenericType(data.GetType());
            return Activator.CreateInstance(EventType, new object[] { data, audit })
                as IGangEvent;
        }

        public static IImmutableList<IGangEvent> SequenceFrom(
            IEnumerable<object> data,
            GangAudit audit
        )
        {
            var snStart = (audit.SequenceNumber ?? 0) + 1;
            return data
                .Select((d, i) =>
                    From(d, audit.SetSequenceNumber(snStart + (uint)i)))
                .ToImmutableList();
        }
    }

    public sealed class GangEvent<TData> : IGangEvent
    {
        public GangEvent(
            TData data, GangAudit audit)
        {
            Data = data;
            Audit = audit;
        }

        public object Data { get; }
        public GangAudit Audit { get; }
    }
}
