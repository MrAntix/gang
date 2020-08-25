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

        public GangCommandExecutor(
            IGangSerializationService serializer,
            IImmutableDictionary<string, Func<object, GangMessageAudit, Task>> handlers = null)
        {
            _serializer = serializer;
            _handlers = handlers ??
                ImmutableDictionary<string, Func<object, GangMessageAudit, Task>>.Empty;
        }

        public IGangCommandExecutor Register<TCommand>(
            string type, Func<TCommand, GangMessageAudit, Task> handler)
        {
            return new GangCommandExecutor(
                _serializer,
                _handlers.Add(type,
                    (c, a) => handler(_serializer.Map<TCommand>(c), a)
                ));
        }

        public Task ExecuteAsync(
            byte[] data, GangMessageAudit audit)
        {
            var wrapper = _serializer.Deserialize<GangCommandWrapper>(data);

            return _handlers[wrapper.Type](wrapper.Command, audit);
        }
    }
}