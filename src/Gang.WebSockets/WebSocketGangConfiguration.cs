using System;
using System.Linq;
using Gang.WebSockets.Serialization;
using Gang.Contracts;
using Gang.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Gang.WebSockets
{
    public static class WebSocketGangConfiguration
    {
        public static IServiceCollection AddWebSocketGangs(
            this IServiceCollection services)
        {
            services.AddSingleton<ISerializationService, JsonSerializationService>();
            services.AddSingleton<IGangHandler, GangHandler>();
            services.AddTransient<GangCollection>();

            return services;
        }

        public static IApplicationBuilder UseWebSocketGangs(
            this IApplicationBuilder app,
            string path)
        {
            app.UseWebSockets();
            app.Map(path, GangAction);

            return app;
        }

        private static void GangAction(IApplicationBuilder app)
        {
            var serializer = app.ApplicationServices.GetRequiredService<ISerializationService>();
            var handler = app.ApplicationServices.GetRequiredService<IGangHandler>();

            app
                .Use(async (context, next) =>
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var webSocket = new WebSocketGangMember(
                            await context.WebSockets.AcceptWebSocketAsync()
                            );
                        var parameters = GetGangParameters(context.Request.Query);

                        await handler.HandleAsync(parameters, webSocket);
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
