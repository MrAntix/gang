using Gang.Commands;
using Gang.Contracts;
using Gang.Management;
using Gang.Management.Events;
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
            services.AddSingleton<IGangManager, GangManager>();
            services.AddTransient<GangCollection>();
            services.AddSingleton<GangAuthenticationFunc>(
                parameters => Task.FromResult(
                    new GangAuth(
                        parameters.Token.GangToBytes(),
                        null
                        )
                    ));
            services.AddSingleton(
                sp => sp.GetServices<Tuple<Type, Func<IGangManagerEventHandler>>>()
                    .Aggregate(new Dictionary<Type, List<Func<IGangManagerEventHandler>>>(),
                    (p, t) =>
                    {
                        if (!p.ContainsKey(t.Item1))
                        {
                            p.Add(t.Item1, new List<Func<IGangManagerEventHandler>>());
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
            services.AddSingleton<GangAuthenticationFunc>(
                sp => sp.GetRequiredService<T>().AuthenticateAsync);

            return services;
        }

        public static IServiceCollection AddGangAuthenticationHandler(
            this IServiceCollection services,
            GangAuthenticationFunc authenticate)
        {
            services.AddSingleton(authenticate);

            return services;
        }

        public static IServiceCollection AddGangEventHandler<TEvent, T>(
            this IServiceCollection services)
            where TEvent : GangManagerEvent
            where T : GangManagerEventHandlerBase<TEvent>
        {
            services.AddTransient<T>();
            services.AddTransient(typeof(Tuple<Type, Func<IGangManagerEventHandler>>),
                sp => Tuple.Create<Type, Func<IGangManagerEventHandler>>(typeof(TEvent), sp.GetRequiredService<T>));

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
                                   select new
                                   {
                                       commandType = gt[1],
                                       handlerService = i,
                                       handlerImplementation = t
                                   }).ToArray()
            )
            {
                services
                    .AddTransient(types.handlerService, types.handlerImplementation)
                    .AddSingleton(
                        sp =>
                        {
                            var serializer = sp.GetRequiredService<IGangSerializationService>();
                            var handle = handlerService
                                .MakeGenericType(hostType, types.commandType)
                                .GetMethod(nameof(IGangCommandHandler<THost, object>.HandleAsync));

                            return GangCommandExecutorFunc<THost>
                                .From(
                                    () => (THost h, object c, GangMessageAudit a) =>
                                        (Task)handle.Invoke(
                                             sp.GetRequiredService(types.handlerService),
                                            new object[] { h, c, a }),
                                    o => serializer.Map(o, types.commandType),
                                    types.commandType.GetCommandTypeName()
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
