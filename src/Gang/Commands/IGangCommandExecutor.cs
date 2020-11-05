using Gang.Contracts;
using System;
using System.Threading.Tasks;

namespace Gang.Commands
{
    public interface IGangCommandExecutor<THost>
        where THost : GangHostBase
    {
        /// <summary>
        /// Register a local method as a command handler
        /// </summary>
        IGangCommandExecutor<THost> RegisterHandler<TCommand>(
            Func<TCommand, GangAudit, Task> handler, string typeName = null);

        /// <summary>
        /// Register a local method as a command handler
        /// </summary>
        IGangCommandExecutor<THost> RegisterHandler<TCommand>(
            Func<TCommand, Task> handler, string typeName = null);

        /// <summary>
        /// Register a command handler provider
        /// </summary>
        IGangCommandExecutor<THost> RegisterHandlerProvider<TCommand>(
            Func<IGangCommandHandler<THost, TCommand>> provider, string typeName = null);

        /// <summary>
        /// Register a local method as a command error handler
        /// </summary>
        IGangCommandExecutor<THost> RegisterErrorHandler(
            Func<byte[], GangAudit, Exception, Task> errorHandler);

        /// <summary>
        /// Execute a command
        /// </summary>
        Task ExecuteAsync(THost host, byte[] data, GangAudit audit);
    }
}