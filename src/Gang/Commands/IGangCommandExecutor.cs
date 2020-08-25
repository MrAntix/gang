using System;
using System.Threading.Tasks;

namespace Gang.Commands
{
    public interface IGangCommandExecutor
    {
        IGangCommandExecutor Register<TCommand>(
            string type, Func<TCommand, GangMessageAudit, Task> handler);

        Task ExecuteAsync(
            byte[] data, GangMessageAudit audit);
    }
}