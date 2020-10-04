using Gang.Commands;
using Gang.Contracts;
using Gang.Events;
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

        public static IServiceCollection AddGangCommandHandler<THost, THandler>(
             this IServiceCollection services)
            where THandler : class, IGangCommandHandler<THost>
            where THost : GangHostBase
        {
            return services
                .AddTransient<THandler>()
                .AddSingleton<Func<IGangCommandHandler<THost>>>(sp => sp.GetRequiredService<THandler>);
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

            var handlerService = typeof(IGangCommandHandler<THost>);
            var providerService = typeof(Func<>).MakeGenericType(handlerService);

            foreach (var handlerType in assembly.GetTypes()
                .Where(t => handlerService.IsAssignableFrom(t))
                .ToArray()
            )
            {
                services
                    .AddTransient(handlerType)
                    .AddSingleton<Func<IGangCommandHandler<THost>>>(
                        sp => () => (IGangCommandHandler<THost>)sp.GetRequiredService(handlerType));
            }

            return services;
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
