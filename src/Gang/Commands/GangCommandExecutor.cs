using Gang.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Gang.Commands
{
    public class GangCommandExecutor<THost>
    {
        readonly THost _host;
        readonly IGangSerializationService _serializer;
        readonly Func<byte[], GangMessageAudit, Exception, Task> _errorHandler;
        readonly IImmutableDictionary<string, Func<object, GangMessageAudit, Task>> _commandHandlers;

        public GangCommandExecutor(
            THost host,
            IGangSerializationService serializer,
            Func<byte[], GangMessageAudit, Exception, Task> errorHandler = null,
            IImmutableDictionary<string, Func<object, GangMessageAudit, Task>> commandHandlers = null)
        {
            _host = host;
            _serializer = serializer;
            _errorHandler = errorHandler;
            _commandHandlers = commandHandlers ??
                ImmutableDictionary<string, Func<object, GangMessageAudit, Task>>.Empty;
        }

        public async Task ExecuteAsync(byte[] data, GangMessageAudit audit)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));
            if (audit is null) throw new ArgumentNullException(nameof(audit));

            try
            {
                var wrapper = _serializer.Deserialize<GangMessageWrapper>(data);

                await _commandHandlers[wrapper.Type](wrapper.Command, audit);
            }
            catch (Exception ex)
            {
                if (_errorHandler == null) throw;

                await _errorHandler(data, audit, ex);
            }
        }

        public GangCommandExecutor<THost> RegisterErrorHandler(
            Func<byte[], GangMessageAudit, Exception, Task> errorHandler)
        {
            if (errorHandler is null) throw new ArgumentNullException(nameof(errorHandler));

            return new GangCommandExecutor<THost>(_host, _serializer,
                errorHandler,
                _commandHandlers
                );
        }

        public GangCommandExecutor<THost> Register(
            Func<IGangCommandHandler<THost>> provider)
        {
            if (provider is null) throw new ArgumentNullException(nameof(provider));

            return new GangCommandExecutor<THost>(_host, _serializer,
                _errorHandler,
               _commandHandlers.Add(
                   provider().CommandTypeName,
                    (c, a) =>
                    {
                        var h = provider();
                        return h.HandleAsync(_host, _serializer.Map(c, h.CommandType), a);
                    })
               );
        }

        public GangCommandExecutor<THost> Register<TCommandHandler>(
            Func<TCommandHandler> provider)
            where TCommandHandler : IGangCommandHandler<THost>
        {
            if (provider is null) throw new ArgumentNullException(nameof(provider));

            return new GangCommandExecutor<THost>(_host, _serializer,
                _errorHandler,
               _commandHandlers.Add(
                   provider().CommandTypeName,
                    (c, a) =>
                    {
                        var h = provider();
                        return h.HandleAsync(_host, _serializer.Map(c, h.CommandType), a);
                    })
               );
        }

        public GangCommandExecutor<THost> Register<TCommandHandler>()
            where TCommandHandler : IGangCommandHandler<THost>, new()
        {
            return Register(() => new TCommandHandler());
        }

        public GangCommandExecutor<THost> Register(
            IEnumerable<Func<IGangCommandHandler<THost>>> providers)
        {
            return providers?.Aggregate(this, (c, i) => c.Register(i))
                ?? throw new ArgumentNullException(nameof(providers));
        }

        public GangCommandExecutor<THost> Register<TCommand>(
            string type,
            Func<TCommand, GangMessageAudit, Task> handler
            )
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException($"'{nameof(type)}' cannot be null or whitespace", nameof(type));
            if (handler is null) throw new ArgumentNullException(nameof(handler));

            return new GangCommandExecutor<THost>(_host, _serializer,
                _errorHandler,
                _commandHandlers.Add(type, (c, a) => handler(_serializer.Map<TCommand>(c), a))
                );
        }

        public GangCommandExecutor<THost> Register<TCommand>(
            string type,
            Func<TCommand, Task> handler
            )
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException($"'{nameof(type)}' cannot be null or whitespace", nameof(type));
            if (handler is null) throw new ArgumentNullException(nameof(handler));

            return Register<TCommand>(type, (c, _) => handler(c));
        }
    }
}