using Gang.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Gang.Commands
{
    public sealed class GangCommandExecutor<THost> :
        IGangCommandExecutor<THost>
        where THost : GangHostBase
    {
        readonly IGangSerializationService _serializer;
        readonly IImmutableDictionary<string, GangCommandExecutorFunc<THost>> _handlerProviders;
        readonly Func<byte[], GangMessageAudit, Exception, Task> _errorHandler;

        public GangCommandExecutor(
            IGangSerializationService serializer,
            IEnumerable<GangCommandExecutorFunc<THost>> handlerProviders = null,
            Func<byte[], GangMessageAudit, Exception, Task> errorHandler = null) :
            this(serializer, handlerProviders?.ToImmutableDictionary(g => g.Name, g => g), errorHandler)
        {
        }

        GangCommandExecutor(
            IGangSerializationService serializer,
            IEnumerable<KeyValuePair<string, GangCommandExecutorFunc<THost>>> handlerProviders,
            Func<byte[], GangMessageAudit, Exception, Task> errorHandler)
        {
            _serializer = serializer;
            _handlerProviders = handlerProviders?.ToImmutableDictionary() ??
                ImmutableDictionary<string, GangCommandExecutorFunc<THost>>.Empty;
            _errorHandler = errorHandler;
        }

        async Task IGangCommandExecutor<THost>.ExecuteAsync(
            THost host, byte[] data, GangMessageAudit audit)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));
            if (audit is null) throw new ArgumentNullException(nameof(audit));

            try
            {
                var wrapper = _serializer.Deserialize<GangMessageWrapper>(data);
                var func = _handlerProviders[wrapper.Type];

                await func.ExecuteAsync(host, wrapper.Command, audit);
            }
            catch (Exception ex)
            {
                if (_errorHandler == null) throw;

                await _errorHandler(data, audit, ex);
            }
        }

        IGangCommandExecutor<THost> IGangCommandExecutor<THost>.RegisterHandler<TCommand>(
            Func<TCommand, GangMessageAudit, Task> handle, string typeName)
        {

            return RegisterHandler(handle, typeName);
        }

        IGangCommandExecutor<THost> IGangCommandExecutor<THost>.RegisterHandler<TCommand>(
            Func<TCommand, Task> handle, string typeName)
        {
            if (handle is null) throw new ArgumentNullException(nameof(handle));

            return RegisterHandler<TCommand>((c, _) => handle(c), typeName);
        }

        IGangCommandExecutor<THost> RegisterHandler<TCommand>(
            Func<TCommand, GangMessageAudit, Task> handle, string typeName)
        {
            if (handle is null) throw new ArgumentNullException(nameof(handle));

            var func = GangCommandExecutorFunc<THost>.From(
                () => (_, o, a) => handle((TCommand)o, a),
                o => _serializer.Map<TCommand>(o),
                typeName ?? typeof(TCommand).GetCommandTypeName()
                );

            return new GangCommandExecutor<THost>(
                _serializer,
                _handlerProviders.Add(func.Name, func),
                _errorHandler);
        }

        IGangCommandExecutor<THost> IGangCommandExecutor<THost>.RegisterHandlerProvider<TCommand>(
            Func<IGangCommandHandler<THost, TCommand>> provider, string typeName)
        {
            if (provider is null) throw new ArgumentNullException(nameof(provider));

            var func = GangCommandExecutorFunc<THost>.From(
                provider,
                o => _serializer.Map<TCommand>(o),
                typeName
                );

            return new GangCommandExecutor<THost>(
                _serializer,
                _handlerProviders.Add(func.Name, func),
                _errorHandler);
        }

        IGangCommandExecutor<THost> IGangCommandExecutor<THost>.RegisterErrorHandler(
            Func<byte[], GangMessageAudit, Exception, Task> errorHandler)
        {
            if (errorHandler is null) throw new ArgumentNullException(nameof(errorHandler));

            return new GangCommandExecutor<THost>(
                _serializer,
                _handlerProviders,
                errorHandler);
        }
    }
}