using Gang.Commands;
using Gang.Contracts;
using System;
using System.Threading.Tasks;

namespace Gang.Tests.StatefulHost
{
    public sealed class SetCommandHandler :
        IGangCommandHandler<FakeGangStatefulHost, SetCommand>
    {
        async Task IGangCommandHandler<FakeGangStatefulHost, SetCommand>
            .HandleAsync(FakeGangStatefulHost host, SetCommand command, GangMessageAudit audit)
        {
            if (host is null) throw new ArgumentNullException(nameof(host));
            if (command is null) throw new ArgumentNullException(nameof(command));
            if (audit is null) throw new ArgumentNullException(nameof(audit));

            await host.SetCount(command.Value, audit);
        }
    }
}
