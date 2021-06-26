using Gang.Commands;
using Gang.Events;
using Gang.Management;
using Gang.Management.Events;
using System;
using System.Threading.Tasks;

namespace Gang.Demo.Web.Server.Events
{
    public sealed class GangProgressHandler :
        IGangEventHandler<GangManagerEvent<GangProgressState>>
    {
        readonly IGangManager _gangManager;

        public GangProgressHandler(
            IGangManager gangManager
            )
        {
            _gangManager = gangManager;
        }

        async Task IGangEventHandler<GangManagerEvent<GangProgressState>>
            .HandleAsync(GangManagerEvent<GangProgressState> e)
        {
            var percent = Math.Round((decimal)e.Data.Index / e.Data.Count * 10000) / 100;
            Console.WriteLine($"{percent} {e.Data.Index}/{e.Data.Count}");

            var host = _gangManager.GangById(e.Audit.GangId).HostMember;
            await host.Controller.SendCommandAsync(
                e.Data
                );
        }
    }
}
