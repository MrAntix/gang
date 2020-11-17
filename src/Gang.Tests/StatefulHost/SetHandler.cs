using Antix.Handlers;
using Gang.Commands;
using System;
using System.Threading.Tasks;

namespace Gang.Tests.StatefulHost
{
    public sealed class SetHandler :
        IHandler<GangCommand<SetCount>, FakeGangStatefulHost>
    {
        async Task IHandler<GangCommand<SetCount>, FakeGangStatefulHost>
            .HandleAsync(GangCommand<SetCount> command, FakeGangStatefulHost host)
        {
            if (host is null) throw new ArgumentNullException(nameof(host));
            if (command is null) throw new ArgumentNullException(nameof(command));

            await host.SetCount(command.Data.Value, command.Audit);
        }
    }
}
