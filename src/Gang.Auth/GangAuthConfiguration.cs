using Gang.Auth.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Gang.Auth
{
    public static class GangAuthConfiguration
    {
        /// <summary>
        /// Add Gang Authentication services
        ///
        /// implementation of IGangAuthUserStore, TUserStore, is added as a singleton
        /// </summary>
        /// <typeparam name="TUserStore">User store type</typeparam>
        /// <param name="services">Serice collection</param>
        /// <param name="settings">Setting</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddGangAuthServices<TUserStore>(
            this IServiceCollection services,
            GangAuthSettings settings
            )
            where TUserStore : class, IGangAuthUserStore
        {
            return services
                .AddSingleton<IGangAuthUserStore, TUserStore>()
                .AddGangAuthServices(settings);
        }

        /// <summary>
        /// Add Gang Authentication services
        ///
        /// n.b. you will need to add an implementation of IGangAuthUserStore
        /// </summary>
        /// <param name="services">Serice collection</param>
        /// <param name="settings">Setting</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddGangAuthServices(
            this IServiceCollection services,
            GangAuthSettings settings
            )
        {
            services
                .AddMvcCore(o =>
                {
                    o.EnableEndpointRouting = false;
                })
                .AddApplicationPart(typeof(GangAuthController).Assembly)
                .AddControllersAsServices();

            return services
                .AddSingleton(settings)
                .AddTransient<IGangAuthService, GangAuthService>()
                .AddTransient<IGangTokenService, GangTokenService>();
        }

        /// <summary>
        /// Use the Auth Api
        /// </summary>
        /// <param name="app">Applicatin builder</param>
        /// <returns>Applicatin builder</returns>
        public static IApplicationBuilder UseGangAuthApi(
            this IApplicationBuilder app)
        {
            return app
                .UseMvc();
        }
    }
}
