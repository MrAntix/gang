using Gang.Authentication.Api;
using Gang.Authentication.Crypto;
using Gang.Authentication.Crypto.Verification;
using Gang.Authentication.Tokens;
using Gang.Authentication.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
            IGangAuthenticationSettings settings
            )
            where TUserStore : class, IGangUserStore
        {
            return services
                .AddSingleton<IGangUserStore, TUserStore>()
                .AddGangAuthenticationServices(settings);
        }

        /// <summary>
        /// Add Gang Authentication services
        /// </summary>
        /// <param name="services">Serice collection</param>
        /// <param name="settings">Setting</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddGangAuthenticationServices(
            this IServiceCollection services,
            IGangAuthenticationSettings settings
            )
        {
            services.TryAddSingleton<IGangUserStore, GangUserStore>();

            services
                .AddMvcCore(o =>
                {
                    o.EnableEndpointRouting = false;
                })
                .AddApplicationPart(typeof(GangAuthenticationController).Assembly)
                .AddControllersAsServices()
                .AddNewtonsoftJson();

            return services
                .AddSingleton(settings)
                .AddGangAuthenticationHandler<GangAuthenticationHandler>()
                .AddTransient<IGangAuthenticationService, GangAuthenticationService>()
                .AddSingleton<IGangCryptoSettings>(settings)
                .AddTransient<IGangCryptoService, GangCryptoService>()
                .AddTransient<IGangCryptoVerificationService, ECES256VerificationService>()
                .AddTransient<IGangCryptoVerificationService, RSARS256VerificationService>()
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
