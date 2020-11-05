using Gang.Contracts;
using System;
using System.Threading.Tasks;

namespace Gang.Commands
{
    public sealed class GangCommandExecutorFunc<THost>
        where THost : GangHostBase
    {
        readonly GangCommandHandlerProvider<THost> _provider;
        readonly Func<object, object> _map;

        public GangCommandExecutorFunc(
            GangCommandHandlerProvider<THost> provider,
            Func<object, object> map,
            string name
            )
        {
            _provider = provider;
            _map = map;
            Name = name;
        }

        public string Name { get; }
        public Task ExecuteAsync(
            THost host, object command, GangAudit audit)
        {
            var typedCommand = _map(command);
            var handler = _provider();

            return handler(host, typedCommand, audit);
        }

        public static GangCommandExecutorFunc<THost> From<TCommand>(
            Func<IGangCommandHandler<THost, TCommand>> provider,
            Func<object, TCommand> map,
            string typeName = null
            )
        {
            if (provider is null) throw new ArgumentNullException(nameof(provider));

            return new GangCommandExecutorFunc<THost>(
                () => (h, o, a) => provider().HandleAsync(h, (TCommand)o, a),
                o => map(o),
                typeName ?? typeof(TCommand).GetCommandTypeName()
                );
        }

        public static GangCommandExecutorFunc<THost> From<TCommand>(
            GangCommandHandlerProvider<THost> provider,
            Func<object, TCommand> map,
            string typeName = null
            )
        {
            if (provider is null) throw new ArgumentNullException(nameof(provider));

            return new GangCommandExecutorFunc<THost>(
                provider,
                o => map(o),
                typeName ?? typeof(TCommand).GetCommandTypeName()
                );
        }
    }
}
