using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Gang.Events
{
    public static class GangEventsConfiguration
    {
        /// <summary>
        /// Add all handlers for the given data type in the same assembly as the data type
        /// </summary>
        /// <typeparam name="TDataImplements">Type implemented by the data types</typeparam>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddHandlers<TDataImplements>(
            this IServiceCollection services)
        {
            return services
                .AddHandlersInAssembly<TDataImplements, TDataImplements>();
        }

        /// <summary>
        /// Add all handlers for the given data type in the same assembly as the TAssemblyOf type
        /// </summary>
        /// <typeparam name="TDataImplements">Type implemented by the data types</typeparam>
        /// <typeparam name="TAssemblyOf">A Type in the target assembly</typeparam>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddHandlersInAssembly<TDataImplements, TAssemblyOf>(
            this IServiceCollection services
            )
        {
            services.TryAddTransient<GangEventExecutor<TDataImplements>>();

            var dataType = typeof(TDataImplements);
            var handlerType = typeof(IGangEventHandler<>);
            var handleMethodName = nameof(IGangEventHandler<object>.HandleAsync);

            foreach (var info in (from implementation in typeof(TAssemblyOf).Assembly.GetTypes()
                                  from service in implementation.GetInterfaces()
                                  where service.IsGenericType
                                          && service.GetGenericTypeDefinition() == handlerType
                                  let arguments = service.GetGenericArguments()
                                  where arguments[0].IsAssignableTo(dataType)
                                  select new
                                  {
                                      data = arguments[0],
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

                        return new GangEventHandler<TDataImplements>(
                            info.data,
                            (data) =>
                            {
                                try
                                {
                                    return (Task)handle.Invoke(
                                          sp.GetRequiredService(info.implementation),
                                          new object[] { data }
                                          );
                                }
                                catch (TargetInvocationException tiex)
                                {

                                    throw new GangEventHandlerException<TDataImplements>(data, tiex);
                                }
                            });
                    });
            }

            return services;
        }
    }
}
