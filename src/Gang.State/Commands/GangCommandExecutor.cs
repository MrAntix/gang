using Gang.Commands;
using Gang.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Gang.State.Commands
{
    public class GangCommandExecutor<TStateData> :
        IGangCommandExecutor<TStateData>
        where TStateData : class, new()
    {
        readonly IGangSerializationService _serialization;

        readonly IImmutableDictionary<string, GangCommandHandler<TStateData>> _handlers;

        public GangCommandExecutor(
            IGangSerializationService serialization,
            IEnumerable<GangCommandHandler<TStateData>> handlers = null)
        {

            _handlers = handlers
                ?.ToImmutableDictionary(h => h.DataType.GetCommandTypeName())
                ?? ImmutableDictionary<string, GangCommandHandler<TStateData>>.Empty;

            _serialization = serialization;
        }

        async Task<GangState<TStateData>> IGangCommandExecutor<TStateData>
            .ExecuteAsync(GangState<TStateData> state, byte[] bytes, GangAudit audit)
        {
            var wrapper = _serialization.Deserialize<GangCommandWrapper>(bytes);

            if (!_handlers.ContainsKey(wrapper.Type))
                throw new GangCommandHandlerNotFoundExcetion();

            var handler = _handlers[wrapper.Type];

            var command = GangCommand.From(
                _serialization.Map(wrapper.Data, handler.DataType),
                audit);

            return await handler.HandleAsync(state, command);
        }

        IGangCommandExecutor<TStateData> IGangCommandExecutor<TStateData>
            .RegisterHandler<TCommandData>(GangCommandHandler<TStateData> handler)
        {
            return new GangCommandExecutor<TStateData>(
                _serialization,
                _handlers.Values.Append(handler)
            );
        }

        IGangCommandExecutor<TStateData> IGangCommandExecutor<TStateData>
            .RegisterHandlerProvider<TCommandData>(Func<IGangCommandHandler<TStateData, TCommandData>> provider)
        {
            return new GangCommandExecutor<TStateData>(
                _serialization,
                _handlers.Values.Append(
                    GangCommandHandler<TStateData>.From(
                       (GangState<TStateData> state, GangCommand<TCommandData> command)
                            => provider().HandleAsync(state, command))
                    )
                );
        }
    }
}