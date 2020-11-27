using Gang.State.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Gang.State
{
    public static class GangStateConfiguration
    {
        /// <summary>
        /// Adds services for TStateData, including;
        ///
        /// Command Executor
        /// Command handlers in same assembly as TStateData
        /// 
        /// </summary>
        /// <typeparam name="TStateData">State</typeparam>
        /// <param name="services">Services collection</param>
        /// <returns>Services collection</returns>
        public static IServiceCollection AddGangState<TStateData>(
            this IServiceCollection services
            )
            where TStateData : class, new()
        {
            return services
                .AddGangCommandExecutor<TStateData>()
                .AddGangCommandHandlers<TStateData>();
        }

        public static IServiceCollection AddGangCommandExecutor<TStateData>(
            this IServiceCollection services
            )
            where TStateData : class, new()
        {
            services.TryAddSingleton<IGangCommandExecutor<TStateData>, GangCommandExecutor<TStateData>>();

            return services;
        }

        public static IServiceCollection AddGangCommandHandlers<TStateData, TAssemblyOf>(
            this IServiceCollection services
            )
            where TStateData : class, new()
        {
            var stateType = typeof(TStateData);
            var assembly = typeof(TAssemblyOf).Assembly;
            var handlerType = typeof(IGangCommandHandler<,>);
            var handleMethodName = nameof(IGangCommandHandler<TStateData, object>.HandleAsync);

            foreach (var info in (from implementation in assembly.GetTypes()
                                  from service in implementation.GetInterfaces()
                                  where service.IsGenericType
                                          && service.GetGenericTypeDefinition() == handlerType
                                  let arguments = service.GetGenericArguments()
                                  where arguments[0] == stateType
                                  select new
                                  {
                                      data = arguments[1],
                                      service,
                                      implementation
                                  }).ToArray())
            {
                services.AddTransient(info.service, info.implementation);
                services.AddTransient(info.implementation);
                services.AddSingleton(
                    sp =>
                    {
                        var handle = info.service
                            .GetMethod(handleMethodName);

                        return new GangCommandHandler<TStateData>(
                            info.data,
                            async (state, data) =>
                            {
                                try
                                {
                                    return await (Task<GangState<TStateData>>)handle.Invoke(
                                          sp.GetRequiredService(info.implementation),
                                          new object[] { state, data }
                                          );
                                }
                                catch (TargetInvocationException tiex)
                                {
                                    throw new GangCommandHandlerException(data, tiex);
                                }
                            });
                    });
            }

            return services;
        }

        public static IServiceCollection AddGangCommandHandlers<TStateData>(
            this IServiceCollection services
            )
            where TStateData : class, new()
        {
            return services
                .AddGangCommandHandlers<TStateData, TStateData>();
        }
    }
}
