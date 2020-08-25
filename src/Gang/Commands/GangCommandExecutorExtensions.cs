using System;
using System.Threading.Tasks;

namespace Gang.Commands
{
    public static class GangCommandExecutorExtensions
    {
        public static IGangCommandExecutor Register<TCommand>(
            this IGangCommandExecutor executor,
            string type, Func<TCommand, Task> handler)
        {
            return executor.Register<TCommand>(type, (command, _) => handler(command));
        }
    }
}