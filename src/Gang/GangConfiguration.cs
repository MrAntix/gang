using Gang.Authentication;
using Gang.Events;
using Gang.Management;
using Gang.Management.Events;
using Gang.Storage;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Gang
{
    public static class GangConfiguration
    {
        public static IServiceCollection AddGangs(
            this IServiceCollection services,
            IGangSettings settings)
        {
            services.AddSingleton(settings);
            services.AddSingleton<IGangManager, GangManager>();
            services.AddSingleton<IGangControllerFactory, GangControllerFactory>();
            services.AddTransient<GangCollection>();
            services.AddSingleton<IGangManagerSequenceProvider, GangManagerInMemorySequenceProvider>();

            // default auth func, just uses the token as the user id
            services.AddSingleton<GangAuthenticationFunc>(
                parameters => Task.FromResult(
                    new GangSession(
                        parameters.Token,
                        null
                        )
                    ));
            return services;
        }

        public static IServiceCollection AddGangManagerEventHandlers<TAssemblyOf>(
             this IServiceCollection services)
        {
            return services
                .AddHandlersInAssembly<IGangManagerEvent, TAssemblyOf>();
        }

        public static IServiceCollection AddGangHost<THost>(
            this IServiceCollection services)
            where THost : GangHostBase
        {
            return services.AddGangFactory<THost>();
        }

        public static IServiceCollection AddGangAuthenticationHandler<T>(
            this IServiceCollection services)
            where T : class, IGangAuthenticationHandler
        {
            services.AddTransient<T>();
            services.AddSingleton<GangAuthenticationFunc>(
                sp => sp.GetRequiredService<T>().HandleAsync);

            return services;
        }

        public static IServiceCollection AddGangAuthenticationHandler(
            this IServiceCollection services,
            GangAuthenticationFunc authenticate)
        {
            services.AddSingleton(authenticate);

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

        public static IServiceCollection AddGangStoreFactory<TFactory>(
            this IServiceCollection services)
            where TFactory : class, IGangStoreFactory
        {
            services.AddSingleton<IGangStoreFactory, TFactory>();

            return services;
        }

        public static IServiceCollection AddInMemoryGangStoreFactory(
            this IServiceCollection services)
        {
            services.AddGangStoreFactory<InMemoryGangStoreFactory>();

            return services;
        }

        public static IServiceCollection AddFileSystemGangStore(
            this IServiceCollection services,
            IFileSystemGangStoreSettings settings
            )
        {
            services.AddSingleton<IMemoryCache>(
                _ => new MemoryCache(new MemoryCacheOptions())
                );
            services.AddSingleton(settings);
            services.AddGangStoreFactory<FileSystemGangStoreFactory>();

            return services;
        }
    }
}
