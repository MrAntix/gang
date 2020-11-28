using Gang.Authentication;
using Gang.Authentication.Users;
using Gang.Management;
using Gang.State;
using Gang.Web.Properties;
using Gang.Web.Services;
using Gang.Web.Services.State;
using Gang.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Reactive.Linq;

namespace Gang.Web
{
    public sealed class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Settings = configuration.Get<Settings>();
        }

        public Settings Settings { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWebSocketGangs()
                .AddGangHost<WebGangHost>()
                .AddGangCommmands<WebGangAggregate>()
                .AddGangManagerEventHandlers<WebGangAddedEventHandler>()
                .AddGangAuthenticationHandler<WebGangAuthenticationHandler>()
                .AddGangAuthenticationServices(Settings.Auth)
                .AddSingleton(Settings.App)
                .AddSingleton(Settings.Smtp)
                .AddTransient<IWebGangSmtpService, WebGangSmtpService>()
                .AddSingleton<IGangAuthenticationUserStore, WebGangAuthenticationUserStore>();
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
                .Map("/disconnect", HandleDisconnect)
                .UseGangAuthenticationApi();
        }

        void HandleDisconnect(
            IApplicationBuilder app)
        {
            var gangManager = app.ApplicationServices
                .GetRequiredService<IGangManager>();

            app.Run(async context =>
            {
                var gangId = context.Request.Query["gangId"].FirstOrDefault();
                var memberId = context.Request.Query["memberId"].FirstOrDefault();

                var gang = gangManager.GangById(gangId);
                var member = gang.MemberById(memberId);

                await member.DisconnectAsync();
            });
        }
    }
}
