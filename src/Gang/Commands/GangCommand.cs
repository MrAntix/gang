using System;

namespace Gang.Commands
{
    public static class GangCommand
    {
        public static IGangCommand From(object data, GangAudit audit)
        {
            var commandType = typeof(GangCommand<>).MakeGenericType(data.GetType());
            return Activator.CreateInstance(commandType, new object[] { data, audit })
                as IGangCommand;
        }
    }

    public sealed class GangCommand<TData>
        : IGangCommand
    {
        public GangCommand(
            TData data,
            GangAudit audit)
        {
            Data = data;
            Audit = audit;
        }

        public TData Data { get; }
        public GangAudit Audit { get; }

        object IGangCommand.Data => Data;
    }
}