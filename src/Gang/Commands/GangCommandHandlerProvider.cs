using Gang.Contracts;
using System;
using System.Threading.Tasks;

namespace Gang.Commands
{
    public delegate Func<THost, object, GangAudit, Task> GangCommandHandlerProvider<THost>();
}
