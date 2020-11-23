using Antix.Handlers;
using System;
using System.Threading.Tasks;

namespace Gang.Commands
{
    public interface IGangCommandExecutor<THost>
        where THost : GangHostBase
    {
        Task ExecuteAsync(THost host, byte[] bytes, GangAudit audit);
        IGangCommandExecutor<THost> RegisterHandler<TData>(Func<TData, GangAudit, Task> handler);
        IGangCommandExecutor<THost> RegisterHandler<TData>(Func<TData, Task> handler);
        IGangCommandExecutor<THost> RegisterHandlerProvider<TData>(Func<IHandler<GangCommand<TData>, THost>> provider);
    }
}