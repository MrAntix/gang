using Gang.Authentication;
using Gang.Management;
using Gang.Serialization;
using Gang.WebSockets.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Gang.WebSockets
{
    public static class WebSocketGangConfiguration
    {
        public const string RESULT_DENIED = "denied";

        public static IServiceCollection AddWebSocketGangs(
            this IServiceCollection services,
            IGangSettings settings
            )
        {
            services.AddGangs(settings);
            services.TryAddSingleton<IGangSerializationService, WebSocketGangJsonSerializationService>();

            return services;
        }

        public static IServiceCollection AddWebSocketGangsSerialization(
            this IServiceCollection services
            )
        {
            return services.AddSingleton<IGangSerializationService, WebSocketGangJsonSerializationService>();
        }

        public static IApplicationBuilder UseWebSocketGangs(
            this IApplicationBuilder app,
            string path
            )
        {
            var manager = app.ApplicationServices.GetRequiredService<IGangManager>();
            var authenticateAsync = app.ApplicationServices.GetRequiredService<GangAuthenticationFunc>();

            app.UseWebSockets();
            app.Map(path, subApp =>
            {
                subApp
                    .Use(async (context, next) =>
                    {
                        if (!context.WebSockets.IsWebSocketRequest)
                        {
                            await next();
                            return;
                        }

                        var parameters = GetGangParameters(context.Request.Query);
                        var session = await authenticateAsync(parameters);
                        var member = await GetGangMemberAsync(session, context);

                        await manager.ManageAsync(parameters, member).BlockAsync();
                    });
            });

            return app;
        }

        static async Task<IGangMember> GetGangMemberAsync(
            GangSession session,
            HttpContext context
            )
        {
            return new WebSocketGangMember(
                $"{Guid.NewGuid():N}".GangToBytes(),
                session,
                await context.WebSockets.AcceptWebSocketAsync()
                );
        }

        public static GangParameters GetGangParameters(
            IQueryCollection query
            )
        {
            if (!TryGetString(query, "gangId", out var gangId))
                return null;

            TryGetString(query, "token", out var token);

            return new GangParameters(gangId, token);
        }

        static bool TryGetString(
            IQueryCollection query,
            string name, out string value
            )
        {
            if (!query.TryGetValue(name, out var values))
            {
                value = default;
                return false;
            }

            value = values.Single();
            return true;
        }
    }
}
