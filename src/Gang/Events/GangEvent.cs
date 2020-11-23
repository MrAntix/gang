using System;

namespace Gang.Events
{
    public static class GangEvent
    {
        public static IGangEvent From(object data, GangAudit audit)
        {
            var EventType = typeof(GangEvent<>).MakeGenericType(data.GetType());
            return Activator.CreateInstance(EventType, new object[] { data, audit })
                as IGangEvent;
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
