using System.Linq;
using System.Threading.Tasks;
using Gang;
using Gang.Contracts;
using Gang.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Antix.Gang.Host
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWebSocketGangs()
                .AddTransient<IGangAuthorizationHandler, HostGangAuthorizationHandler>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

        Task<bool> HandleAuthorizationAsync(
            IApplicationBuilder app, GangParameters parameters)
        {

            return Task.FromResult(true);
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

                await gangHandler
                     .GangById(gangId)
                     .MemberById(memberId)
                     .DisconnectAsync();
            });
        }
    }
}
