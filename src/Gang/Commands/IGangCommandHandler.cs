using System;
using System.Threading.Tasks;

namespace Gang.Commands
{
    public interface IGangCommandHandler<THost>
    {
        string CommandTypeName { get; }
        Type CommandType { get; }
        Task HandleAsync(THost host, object command, GangMessageAudit audit);
    }
}