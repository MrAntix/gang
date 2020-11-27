using System;
using System.Threading.Tasks;

namespace Gang.State.Commands
{
    public static class GangCommandExtensions
    {
        public static IGangCommandExecutor<TStateData> RegisterHandler<TStateData, TCommandData>(
            this IGangCommandExecutor<TStateData> executor,
            Func<GangState<TStateData>, GangCommand<TCommandData>, Task<GangState<TStateData>>> action)
            where TStateData : class, new()
        {
            return executor.RegisterHandler<TCommandData>(
                    GangCommandHandler<TStateData>.From(action)
                );
        }

        public static IGangCommandExecutor<TStateData> RegisterHandler<TStateData, TCommandData>(
            this IGangCommandExecutor<TStateData> executor,
            Func<GangState<TStateData>, GangCommand<TCommandData>, GangState<TStateData>> action)
            where TStateData : class, new()
        {
            return executor.RegisterHandler<TCommandData>(
                    GangCommandHandler<TStateData>.From(action)
                );
        }

        public static IGangCommandExecutor<TStateData> RegisterHandler<TStateData, TCommandData>(
            this IGangCommandExecutor<TStateData> executor,
            Func<GangCommand<TCommandData>, Task<GangState<TStateData>>> action)
            where TStateData : class, new()
        {
            return executor.RegisterHandler<TCommandData>(
                    GangCommandHandler<TStateData>.From(action)
                );
        }

        public static IGangCommandExecutor<TStateData> RegisterHandler<TStateData, TCommandData>(
            this IGangCommandExecutor<TStateData> executor,
            Func<GangCommand<TCommandData>, GangState<TStateData>> action)
            where TStateData : class, new()
        {
            return executor.RegisterHandler<TCommandData>(
                    GangCommandHandler<TStateData>.From(action)
                );
        }
    }
}