using Gang.Contracts;
using Gang.Events;
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
            services.AddSingleton<ISerializationService, JsonSerializationService>();
            services.AddSingleton<IGangHandler, GangHandler>();
            services.AddTransient<GangCollection>();

            return services;
        }

        public static IApplicationBuilder UseWebSocketGangs(
            this IApplicationBuilder app,
            string path,
            Func<IApplicationBuilder, GangParameters, Task<bool>> authorizeAsync = null)
        {
            app.UseWebSockets();
            app.Map(path, _ =>
            {
                var serializer = app.ApplicationServices.GetRequiredService<ISerializationService>();
                var handler = app.ApplicationServices.GetRequiredService<IGangHandler>();
                if (authorizeAsync == null)
                {
                    var authHandler = app.ApplicationServices.GetService<IGangAuthorizationHandler>();
                    if (authHandler != null)
                        authorizeAsync = (a, p) => authHandler.AuthorizeAsync(p);
                }

                app
                    .Use(async (context, next) =>
                    {
                        if (!context.WebSockets.IsWebSocketRequest)
                        {
                            await next();
                            return;
                        }

                        var parameters = GetGangParameters(context.Request.Query);
                        var member = await GetGangMemberAsync(context);
                        if (authorizeAsync != null
                            && !await authorizeAsync(app, parameters))
                        {
                            await member.DisconnectAsync("denied");
                            return;
                        }

                        await handler.HandleAsync(parameters, member);
                    });

            });

            return app;
        }

        static async Task<IGangMember> GetGangMemberAsync(HttpContext context)
        {
            return new WebSocketGangMember(
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
