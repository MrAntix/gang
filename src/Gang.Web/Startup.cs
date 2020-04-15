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
                .AddGangFactory<WebGangHost>()
                .AddGangEventHandler<WebGangAddedEventHandler>()
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
