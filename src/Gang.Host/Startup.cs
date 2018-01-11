using System;
using System.Linq;
using Gang;
using Gang.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Antix.Gang.Host
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWebSocketGangs();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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

        private void HandleDisconnect(IApplicationBuilder app)
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
