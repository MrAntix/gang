using System;
using System.Threading.Tasks;

namespace Gang.Events
{
    public sealed class GangEventHandler<TDataImplements>
    {
        public GangEventHandler(
            Type dataType,
            Func<TDataImplements, Task> handleAsync)
        {
            DataType = dataType;
            HandleAsync = handleAsync;
        }

        public Type DataType { get; }
        public Func<TDataImplements, Task> HandleAsync { get; }

        public static GangEventHandler<TDataImplements> From<TData>(
            IGangEventHandler<TData> GangEventHandler)
            where TData : class, TDataImplements
        {
            return new GangEventHandler<TDataImplements>(
                typeof(TData),
                data =>
                {
                    return GangEventHandler.HandleAsync((TData)data);
                });
        }

        public static GangEventHandler<TDataImplements> From<TData>(
            Action<TData> action)
            where TData : TDataImplements
        {
            return new GangEventHandler<TDataImplements>(
                typeof(TData),
                data =>
                {
                    action((TData)data);
                    return Task.CompletedTask;
                });
        }

        public static GangEventHandler<TDataImplements> From<TData>(
            Func<TData, Task> action)
            where TData : TDataImplements
        {
            return new GangEventHandler<TDataImplements>(
                typeof(TData),
                data =>
                {
                    return action((TData)data);
                });
        }
    }
}
