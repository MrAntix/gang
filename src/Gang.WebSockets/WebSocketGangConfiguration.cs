using Gang.Contracts;
using Gang.Serialization;
using Gang.WebSockets.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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
            services.AddSingleton<IGangSerializationService, WebSocketGangJsonSerializationService>();

            return services;
        }

        public static IApplicationBuilder UseWebSocketGangs(
            this IApplicationBuilder app,
            string path
            )
        {
            var handler = app.ApplicationServices.GetRequiredService<IGangHandler>();
            var authenticateAsync = app.ApplicationServices.GetRequiredService<Func<GangParameters, Task<byte[]>>>();
            var eventHandlerFactories = app.ApplicationServices.GetRequiredService<Dictionary<Type, List<Func<IGangEventHandler>>>>();

            if (eventHandlerFactories?.Any() ?? false)
            {
                handler.Events.Subscribe(e =>
                {
                    var eventType = e.GetType();
                    if (!eventHandlerFactories.ContainsKey(eventType)) return;

                    foreach (var factory in eventHandlerFactories[eventType])
                        factory().HandleAsync(e);
                });
            }

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

                        var gangMemberId = await authenticateAsync(parameters);
                        using var gangMember = await GetGangMemberAsync(gangMemberId, context);

                        if (gangMember.Id != null)
                            await handler.HandleAsync(parameters, gangMember);

                        else
                            await gangMember.DisconnectAsync("denied");

                    });
            });

            return app;
        }

        static async Task<IGangMember> GetGangMemberAsync(
            byte[] id,
            HttpContext context
            )
        {
            return new WebSocketGangMember(
                id,
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
