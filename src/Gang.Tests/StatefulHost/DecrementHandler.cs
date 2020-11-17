using Antix.Handlers;
using Gang.Commands;
using System;
using System.Threading.Tasks;

namespace Gang.Tests.StatefulHost
{
    public sealed class DecrementHandler :
        IHandler<GangCommand<Decrement>, FakeGangStatefulHost>
    {
        async Task IHandler<GangCommand<Decrement>, FakeGangStatefulHost>
             .HandleAsync(GangCommand<Decrement> command,
             FakeGangStatefulHost host)
        {
            if (command is null) throw new ArgumentNullException(nameof(command));
            if (host is null) throw new ArgumentNullException(nameof(host));

            await host.SetCount(host.State.Count - 1, command.Audit);
        }
    }
}
