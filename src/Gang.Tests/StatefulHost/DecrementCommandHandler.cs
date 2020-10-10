using Gang.Commands;
using System;
using System.Threading.Tasks;

namespace Gang.Tests.StatefulHost
{
    public sealed class DecrementCommandHandler :
        IGangCommandHandler<FakeGangStatefulHost, DecrementCommand>
    {
        async Task IGangCommandHandler<FakeGangStatefulHost, DecrementCommand>
            .HandleAsync(FakeGangStatefulHost host, DecrementCommand command, GangMessageAudit audit)
        {
            if (host is null) throw new ArgumentNullException(nameof(host));
            if (audit is null) throw new ArgumentNullException(nameof(audit));

            await host.SetCount(host.State.Count - 1, audit);
        }
    }
}
