using System;
using System.Threading.Tasks;

namespace Gang.Commands
{
    public abstract class GangCommandHandlerBase<THost, TCommand> :
        IGangCommandHandler<THost>
    {
        public abstract string CommandTypeName { get; }

        protected abstract Task HandleAsync(THost host, TCommand command, GangMessageAudit audit);

        Type IGangCommandHandler<THost>
            .CommandType => typeof(TCommand);

        Task IGangCommandHandler<THost>
            .HandleAsync(THost host, object command, GangMessageAudit audit)
        {
            return this.HandleAsync(host, (TCommand)command, audit);
        }
    }
}