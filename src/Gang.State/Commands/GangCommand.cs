using System.Linq;
using System.Reflection;

namespace Gang.State.Commands
{
    public static class GangCommand
    {
        static readonly MethodInfo _from;

        static GangCommand()
        {
            _from = typeof(GangCommand)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == nameof(From) && m.IsGenericMethod);
        }

        public static IGangCommand From(object data, GangAudit audit)
        {
            if (data is null)
                throw new System.ArgumentNullException(nameof(data));

            return (IGangCommand)_from
                .MakeGenericMethod(data.GetType())
                .Invoke(null, new[] { data, audit });
        }

        public static GangCommand<TData> From<TData>(
            TData data, GangAudit audit)
               where TData : class
        {
            if (data is null)
                throw new System.ArgumentNullException(nameof(data));
            if (audit is null)
                throw new System.ArgumentNullException(nameof(audit));

            return new GangCommand<TData>(data, audit);
        }
    }

    public sealed class GangCommand<TData>
        : IGangCommand
        where TData : class
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