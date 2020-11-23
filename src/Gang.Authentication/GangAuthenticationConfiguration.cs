using Gang.Authentication.Api;
using Gang.Authentication.Tokens;
using Gang.Authentication.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Gang.Authentication
{
    public static class GangAuthenticationConfiguration
    {
        /// <summary>
        /// Add Gang Authentication services
        ///
        /// implementation of IGangAuthenticationUserStore, TUserStore, is added as a singleton
        /// </summary>
        /// <typeparam name="TUserStore">User store type</typeparam>
        /// <param name="services">Serice collection</param>
        /// <param name="settings">Setting</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddGangAuthenticationServices<TUserStore>(
            this IServiceCollection services,
            GangAuthenticationSettings settings
            )
            where TUserStore : class, IGangAuthenticationUserStore
        {
            return services
                .AddSingleton<IGangAuthenticationUserStore, TUserStore>()
                .AddGangAuthenticationServices(settings);
        }

        /// <summary>
        /// Add Gang Authentication services
        ///
        /// n.b. you will need to add an implementation of IGangAuthenticationUserStore
        /// </summary>
        /// <param name="services">Serice collection</param>
        /// <param name="settings">Setting</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddGangAuthenticationServices(
            this IServiceCollection services,
            GangAuthenticationSettings settings
            )
        {
            services
                .AddMvcCore(o =>
                {
                    o.EnableEndpointRouting = false;
                })
                .AddApplicationPart(typeof(GangAuthenticationController).Assembly)
                .AddControllersAsServices();

            return services
                .AddSingleton(settings)
                .AddTransient<IGangAuthenticationService, GangAuthenticationService>()
                .AddTransient<IGangTokenService, GangTokenService>();
        }

        /// <summary>
        /// Use the Auth Api
        /// </summary>
        /// <param name="app">Applicatin builder</param>
        /// <returns>Applicatin builder</returns>
        public static IApplicationBuilder UseGangAuthenticationApi(
            this IApplicationBuilder app)
        {
            return app
                .UseMvc();
        }
    }
}
