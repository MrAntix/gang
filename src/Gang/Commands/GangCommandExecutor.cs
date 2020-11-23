using Antix.Handlers;
using Gang.Serialization;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Gang.Commands
{
    public sealed class GangCommandExecutor<THost> :
        IGangCommandExecutor<THost>
        where THost : GangHostBase
    {
        readonly IGangSerializationService _serialization;
        readonly Executor<IGangCommand, THost> _executor;
        readonly ImmutableDictionary<string, Type> _commandTypes;

        public GangCommandExecutor(
            IGangSerializationService serialization,
            Executor<IGangCommand, THost> executor
            )
        {
            _serialization = serialization;
            _executor = executor;
            _commandTypes = executor.DataTypes
                .Select(t => t.GetGenericArguments()[0])
                .ToImmutableDictionary(t => t.GetCommandTypeName());
        }

        public async Task ExecuteAsync(
            THost host, byte[] bytes, GangAudit audit)
        {
            var wrapper = _serialization.Deserialize<GangCommandWrapper>(bytes);
            var type = _commandTypes[wrapper.Type];

            var commandData = _serialization.Map(wrapper.Data, type);

            await _executor
                .ExecuteAsync(GangCommand.From(commandData, audit), host);
        }

        IGangCommandExecutor<THost> RegisterHandler<TData>(
            Func<TData, GangAudit, Task> handler)
        {
            return new GangCommandExecutor<THost>(
                _serialization,
                _executor.AddHandler<GangCommand<TData>>(
                    (command, host) => handler(command.Data, command.Audit)
                    )
                );
        }

        IGangCommandExecutor<THost> IGangCommandExecutor<THost>
            .RegisterHandler<TData>(Func<TData, GangAudit, Task> handler)
        {
            return RegisterHandler(handler);
        }

        IGangCommandExecutor<THost> IGangCommandExecutor<THost>
            .RegisterHandler<TData>(Func<TData, Task> handler)
        {
            return RegisterHandler<TData>((data, _) => handler(data));
        }

        IGangCommandExecutor<THost> IGangCommandExecutor<THost>
            .RegisterHandlerProvider<TData>(Func<IHandler<GangCommand<TData>, THost>> provider)
        {
            return new GangCommandExecutor<THost>(
                _serialization,
                _executor.AddHandler<GangCommand<TData>>(
                    (command, host) => provider().HandleAsync(command, host))
                );
        }
    }
}