using Gang.Auth.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Gang.Auth
{
    public static class GangAuthConfiguration
    {
        public static IServiceCollection AddGangAuthServices(
            this IServiceCollection services,
            GangAuthSettings settings)
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

        public static IApplicationBuilder UseGangAuthApi(
            this IApplicationBuilder app)
        {
            return app
                .UseMvc();
        }
    }
}
