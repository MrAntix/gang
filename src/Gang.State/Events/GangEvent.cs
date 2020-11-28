using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Gang.State.Events
{
    public static class GangEvent
    {
        public static IGangEvent From(
            object data, GangAudit audit, uint sequenceNumber
            )
        {
            var EventType = typeof(GangEvent<>).MakeGenericType(data.GetType());
            return Activator.CreateInstance(EventType, new object[] { data, audit, sequenceNumber })
                as IGangEvent;
        }

        public static IImmutableList<IGangEvent> SequenceFrom(
            IEnumerable<object> data,
            GangAudit audit,
            uint start = 0
        )
        {
            var sequence = audit.Sequence ?? 0U;
            return data
                .Select((d, i) =>
                    From(d, audit.SetSequence(++sequence),
                    start + (uint)i + 1U)
                )
                .ToImmutableList();
        }
    }

    public sealed class GangEvent<TData> : IGangEvent
    {
        public GangEvent(
            TData data, GangAudit audit, uint sequence)
        {
            Data = data;
            Audit = audit;
            Sequence = sequence;
        }

        public object Data { get; }
        public GangAudit Audit { get; }
        public uint Sequence { get; }
    }
}
