using Gang.Authentication;
using Gang.Management;
using Gang.Serialization;
using Gang.WebSockets.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Gang.WebSockets
{
    public static class WebSocketGangConfiguration
    {
        public static IServiceCollection AddWebSocketGangs(
            this IServiceCollection services)
        {
            services.AddGangs();
            services.AddSingleton<IWebSocketGangAuthenticationService, WebSocketGangAuthenticationenticator>();
            services.AddSingleton<IGangSerializationService, WebSocketGangJsonSerializationService>();

            return services;
        }

        public static IApplicationBuilder UseWebSocketGangs(
            this IApplicationBuilder app,
            string path
            )
        {
            var authenticator = app.ApplicationServices.GetRequiredService<IWebSocketGangAuthenticationService>();
            var manager = app.ApplicationServices.GetRequiredService<IGangManager>();

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

                        await authenticator.ExecuteAsync(
                            GetGangParameters(context.Request.Query),
                            auth => GetGangMemberAsync(auth, context));
                    });
            });

            return app;
        }

        static async Task<IGangMember> GetGangMemberAsync(
            GangAuth auth,
            HttpContext context
            )
        {
            return new WebSocketGangMember(
                $"{Guid.NewGuid():N}".GangToBytes(),
                auth,
                await context.WebSockets.AcceptWebSocketAsync()
                );
        }

        public static GangParameters GetGangParameters(IQueryCollection query)
        {
            if (!TryGetString(query, "gangId", out var gangId))
                return null;

            TryGetString(query, "token", out var token);

            return new GangParameters(gangId, token);
        }

        static bool TryGetString(IQueryCollection query, string name, out string value)
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
