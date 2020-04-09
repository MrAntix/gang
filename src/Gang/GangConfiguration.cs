using Microsoft.Extensions.DependencyInjection;
using System;

namespace Gang
{
    public static class GangConfiguration
    {
        public static IServiceCollection AddGangAuthorizationHandler<T>(
                        this IServiceCollection services)
            where T : class, IGangAuthorizationHandler
        {
            services.AddTransient<IGangAuthorizationHandler, T>();

            return services;
        }
    }
}
