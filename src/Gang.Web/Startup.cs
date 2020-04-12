using Gang.Contracts;
using Gang.Events;
using Gang.Web.Services;
using Gang.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace Gang.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWebSocketGangs()
                .AddTransient<WebGangHost>()
                .AddGangAuthenticationHandler<WebGangAuthenticationHandler>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app
                .UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSocketGangs("/ws")
                .Map("/disconnect", HandleDisconnect);

            var gangHandler = app.ApplicationServices
                .GetRequiredService<IGangHandler>();
            gangHandler.Events
                .OfType<GangAddedEvent>()
                .Subscribe(async (e) =>
                {
                    Console.WriteLine("GangAddedEvent");

                    var host = app.ApplicationServices.GetRequiredService<WebGangHost>();
                    await gangHandler.HandleAsync(
                        new GangParameters(e.GangId, null),
                        host);
                });
        }

        void HandleDisconnect(
            IApplicationBuilder app)
        {
            var gangHandler = app.ApplicationServices
                .GetRequiredService<IGangHandler>();

            app.Run(async context =>
            {
                var gangId = context.Request.Query["gangId"].FirstOrDefault();
                var memberId = context.Request.Query["memberId"].FirstOrDefault();

                var gang = gangHandler.GangById(gangId);
                var member = gang.MemberById(memberId);

                await member.DisconnectAsync();
            });
        }
    }
}
