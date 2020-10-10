using System;
using System.Threading.Tasks;

namespace Gang.Commands
{
    public delegate Func<THost, object, GangMessageAudit, Task> GangCommandHandlerProvider<THost>();
}
