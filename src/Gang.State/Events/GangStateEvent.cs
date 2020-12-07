using System;

namespace Gang.State.Events
{
    public static class GangStateEvent
    {
        public static IGangStateEvent From(
            object data,
            GangAudit audit)
        {
            var type = typeof(GangStateEvent<>)
                .MakeGenericType(data.GetType());

            return (IGangStateEvent)Activator.CreateInstance(
                type, data, audit
                );
        }
    }

    public sealed class GangStateEvent<TData> : IGangStateEvent
    {
        public GangStateEvent(
            TData data,
            GangAudit audit
            )
        {
            Data = data;
            Audit = audit;
        }

        public TData Data { get; }
        public GangAudit Audit { get; }

        object IGangStateEvent.Data => Data;
    }
}
