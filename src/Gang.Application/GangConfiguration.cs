using System;
using System.Linq;
using Antix.Gang;
using Gang.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Gang.Application
{
    public static class GangConfiguration
    {
        public static IServiceCollection AddGang(
            this IServiceCollection services)
        {
            services.AddSingleton<IGangWebsocketHandler, GangWebsocketHandler>();

            return services;
        }

        public static IApplicationBuilder UseGang(
            this IApplicationBuilder app,
            string path)
        {
            app.UseWebSockets();
            app.Map(path, GangAction);

            return app;
        }

        private static void GangAction(IApplicationBuilder app)
        {
            var handler = app.ApplicationServices.GetRequiredService<IGangWebsocketHandler>();

            app
                .Use(async (context, next) =>
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        var parameters = GetGangParameters(context.Request.Query);

                        await handler.HandleAsync(webSocket, parameters);
                    }
                    else
                    {
                        await next();
                    }
                });
        }

        public static GangParameters GetGangParameters(IQueryCollection query)
        {
            if (!TryGetString(query, "gangId", out var gangId))
                return null;

            return new GangParameters(gangId);
        }

        static bool TryGetString(IQueryCollection query, string name, out string value)
        {
            if (!query.TryGetValue(name, out var values))
            {
                value = default(string);
                return false;
            }

            value = values.Single();
            return true;
        }

        static bool TryGetGuid(IQueryCollection query, string name, out Guid value)
        {
            if (!query.TryGetValue(name, out var values))
            {
                value = default(Guid);
                return false;
            }

            return Guid.TryParse(values.Single(), out value);
        }
    }
}
