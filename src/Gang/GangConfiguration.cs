using Gang.Commands;
using Gang.Contracts;
using Gang.Events;
using Gang.Serialization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Gang
{
    public static class GangConfiguration
    {
        public static IServiceCollection AddGangs(
            this IServiceCollection services)
        {
            services.AddSingleton<IGangHandler, GangHandler>();
            services.AddTransient<GangCollection>();
            services.AddSingleton<Func<GangParameters, Task<byte[]>>>(
                _ => Task.FromResult($"{Guid.NewGuid():N}".GangToBytes())
                );
            services.AddSingleton(
                sp => sp.GetServices<Tuple<Type, Func<IGangEventHandler>>>()
                    .Aggregate(new Dictionary<Type, List<Func<IGangEventHandler>>>(),
                    (p, t) =>
                    {
                        if (!p.ContainsKey(t.Item1))
                        {
                            p.Add(t.Item1, new List<Func<IGangEventHandler>>());
                        }
                        p[t.Item1].Add(t.Item2);
                        return p;
                    })
                );

            return services;
        }

        public static IServiceCollection AddGangHost<THost>(
            this IServiceCollection services)
            where THost : GangHostBase
        {
            return services.AddGangFactory<THost>()
                .AddGangCommandHandlersForHost<THost>();
        }

        public static IServiceCollection AddGangAuthenticationHandler<T>(
                        this IServiceCollection services)
            where T : class, IGangAuthenticationHandler
        {
            services.AddTransient<T>();
            services.AddSingleton<Func<GangParameters, Task<byte[]>>>(
                sp => sp.GetRequiredService<T>().AuthenticateAsync);

            return services;
        }

        public static IServiceCollection AddGangAuthenticationHandler(
            this IServiceCollection services,
            Func<GangParameters, Task<byte[]>> authenticate)
        {
            services.AddSingleton(authenticate);

            return services;
        }

        public static IServiceCollection AddGangEventHandler<TEvent, T>(
            this IServiceCollection services)
            where TEvent : GangEvent
            where T : GangEventHandlerBase<TEvent>
        {
            services.AddTransient<T>();
            services.AddTransient(typeof(Tuple<Type, Func<IGangEventHandler>>),
                sp => Tuple.Create<Type, Func<IGangEventHandler>>(typeof(TEvent), sp.GetRequiredService<T>));

            return services;
        }

        public static IServiceCollection AddGangCommandHandlersForHost<THost>(
             this IServiceCollection services)
            where THost : GangHostBase
        {
            return services
                .AddGangCommandHandlersForHost<THost>(typeof(THost).Assembly);
        }

        public static IServiceCollection AddGangCommandHandlersForHost<THost, TInAssembly>(
             this IServiceCollection services)
            where THost : GangHostBase
        {
            return services
                .AddGangCommandHandlersForHost<THost>(typeof(TInAssembly).Assembly);
        }

        static IServiceCollection AddGangCommandHandlersForHost<THost>(
             this IServiceCollection services,
             Assembly assembly)
            where THost : GangHostBase
        {
            var hostType = typeof(THost);

            var handlerService = typeof(IGangCommandHandler<,>);
            var providerService = typeof(Func<>).MakeGenericType(handlerService);

            foreach (var types in (from t in assembly.GetTypes()
                                   from i in t.GetInterfaces()
                                   where i.IsGenericType
                                           && i.GetGenericTypeDefinition() == handlerService
                                   let gt = i.GetGenericArguments()
                                   where gt[0] == hostType
                                   select new { commandType = gt[1], handlerType = t }).ToArray()
            )
            {
                services
                    .AddTransient(types.handlerType)
                    .AddSingleton(
                        sp =>
                        {
                            var serializer = sp.GetRequiredService<IGangSerializationService>();
                            var handleMethod = typeof(IGangCommandHandler<,>)
                                .MakeGenericType(hostType, types.commandType)
                                .GetMethod(nameof(IGangCommandHandler<THost, object>.HandleAsync));

                            dynamic provider() => sp.GetRequiredService(types.handlerType);

                            return new GangNamedFunc<GangCommandHandlerProvider<THost>>(
                                types.commandType.GetCommandTypeName(),
                                () => (h, o, a) =>
                                    {
                                        var c = serializer.Map(o, types.commandType);
                                        var handler = provider();

                                        return handleMethod.Invoke(handler, new object[] { h, c, a });
                                    }
                                );
                        }
                     );
            }

            return services.AddSingleton<IGangCommandExecutor<THost>, GangCommandExecutor<THost>>();
        }


        public static IServiceCollection AddGangFactory<T>(
            this IServiceCollection services)
            where T : class
        {
            services.AddTransient<T>();
            services.AddSingleton<Func<T>>(sp => sp.GetRequiredService<T>);

            return services;
        }
    }
}
