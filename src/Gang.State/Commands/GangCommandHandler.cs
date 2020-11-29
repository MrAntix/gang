using System;
using System.Threading.Tasks;

namespace Gang.State.Commands
{
    public sealed class GangCommandHandler<TStateData>
        where TStateData : class, new()
    {
        public GangCommandHandler(
            Type dataType,
            Func<GangState<TStateData>, IGangCommand, Task<GangState<TStateData>>> handleAsync)
        {
            DataType = dataType;
            HandleAsync = handleAsync;
        }

        public Type DataType { get; }
        public Type StateType { get; } = typeof(TStateData);
        public Func<GangState<TStateData>, IGangCommand, Task<GangState<TStateData>>> HandleAsync { get; }

        public static GangCommandHandler<TStateData> From<TCommandData>(
            IGangCommandHandler<TStateData, TCommandData> handler)
            where TCommandData : class
        {
            return new GangCommandHandler<TStateData>(
                typeof(TCommandData),
                (state, command) =>
                    handler.HandleAsync(state, (GangCommand<TCommandData>)command)
                );
        }

        public static GangCommandHandler<TStateData> From<TCommandData>(
            Func<GangCommand<TCommandData>, GangState<TStateData>> action)
            where TCommandData : class
        {
            return new GangCommandHandler<TStateData>(
                typeof(TCommandData),
                (_, command) => Task.FromResult(
                    action((GangCommand<TCommandData>)command)
                    )
                );
        }

        public static GangCommandHandler<TStateData> From<TCommandData>(
            Func<GangCommand<TCommandData>, Task<GangState<TStateData>>> action)
            where TCommandData : class
        {
            return new GangCommandHandler<TStateData>(
                typeof(TCommandData),
                (_, command) =>
                    action((GangCommand<TCommandData>)command)
                );
        }

        public static GangCommandHandler<TStateData> From<TCommandData>(
            Func<GangState<TStateData>, GangCommand<TCommandData>, GangState<TStateData>> action)
            where TCommandData : class
        {
            return new GangCommandHandler<TStateData>(
                typeof(TCommandData),
                (state, command) => Task.FromResult(
                    action(state, (GangCommand<TCommandData>)command)
                    )
                );
        }

        public static GangCommandHandler<TStateData> From<TCommandData>(
            Func<GangState<TStateData>, GangCommand<TCommandData>, Task<GangState<TStateData>>> action)
            where TCommandData : class
        {
            return new GangCommandHandler<TStateData>(
                typeof(TCommandData),
                (state, command) =>
                    action(state, (GangCommand<TCommandData>)command)
                );
        }
    }
}