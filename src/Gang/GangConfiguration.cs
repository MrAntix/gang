using Gang.Contracts;
using Gang.Events;
using Microsoft.Extensions.DependencyInjection;
using System;
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

        public static IServiceCollection AddGangEventHandler<T>(
            this IServiceCollection services)
            where T : class, IGangEventHandler
        {
            services.AddTransient<IGangEventHandler, T>();

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
