using Gang.Contracts;
using Gang.Events;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                _ => Task.FromResult(Encoding.UTF8.GetBytes($"{Guid.NewGuid():N}"))
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
                sp => Tuple.Create<Type, Func<IGangEventHandler>>(typeof(TEvent), () => sp.GetRequiredService<T>()));

            return services;
        }

        public static IServiceCollection AddGangFactory<T>(
            this IServiceCollection services)
            where T : class
        {
            services.AddTransient<T>();
            services.AddSingleton<Func<T>>(sp => () => sp.GetRequiredService<T>());

            return services;
        }
    }
}
