using Gang.Serialization;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Gang.Commands
{
    public sealed class GangCommandExecutor : IGangCommandExecutor
    {
        readonly IGangSerializationService _serializer;
        readonly IImmutableDictionary<string, Func<object, GangMessageAudit, Task>> _handlers;
        readonly Func<byte[], GangMessageAudit, Exception, Task> _errorHandler;

        public GangCommandExecutor(
            IGangSerializationService serializer,
            IImmutableDictionary<string, Func<object, GangMessageAudit, Task>> handlers = null,
            Func<byte[], GangMessageAudit, Exception, Task> errorHandler = null)
        {
            _serializer = serializer;
            _handlers = handlers ??
                ImmutableDictionary<string, Func<object, GangMessageAudit, Task>>.Empty;
            _errorHandler = errorHandler;
        }

        IGangCommandExecutor IGangCommandExecutor.Register<TCommand>(
            string type, Func<TCommand, GangMessageAudit, Task> handler)
        {
            return new GangCommandExecutor(
                _serializer,
                _handlers.Add(type,
                     (c, a) => handler(_serializer.Map<TCommand>(c), a)
                ),
                _errorHandler);
        }

        IGangCommandExecutor IGangCommandExecutor.RegisterErrorHandler(
            Func<byte[], GangMessageAudit, Exception, Task> handler)
        {
            return new GangCommandExecutor(
                _serializer,
                _handlers,
                handler
                );
        }

        async Task IGangCommandExecutor.ExecuteAsync(
            byte[] data, GangMessageAudit audit)
        {
            try
            {
                var wrapper = _serializer.Deserialize<GangCommandWrapper>(data);

                await _handlers[wrapper.Type](wrapper.Command, audit);
            }
            catch (Exception ex)
            {
                if (_errorHandler == null) throw;

                await _errorHandler(data, audit, ex);
            }
        }
    }
}