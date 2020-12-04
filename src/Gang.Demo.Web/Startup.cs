using Gang.Authentication;
using Gang.Demo.Web.Properties;
using Gang.Demo.Web.Services;
using Gang.Demo.Web.Services.Events;
using Gang.Demo.Web.Services.State;
using Gang.Management;
using Gang.State;
using Gang.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Reactive.Linq;

namespace Gang.Demo.Web
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
            services
                .AddSingleton(Settings.App)
                .AddSingleton(Settings.Auth)
                .AddSingleton(Settings.Smtp)
                .AddWebSocketGangs()
                .AddGangManagerEventHandlers<GangAddedHandler>()
                .AddGangHost<HostMember>()
                .AddGangStateInMemory<HostState>()
                .AddTransient<ISmtpService, SmtpService>();

            if (Settings.Auth.Enabled)
            {
                services
                    .AddGangAuthenticationServices<UserStore>(Settings.Auth);
            }
            else
            {
                services
                    .AddGangAuthenticationHandler<AuthenticationHandler>();
            }
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
                //.UseGangAuthenticationApi()
                .UseWebSocketGangs("/ws")
                .Map("/disconnect", HandleDisconnect);
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