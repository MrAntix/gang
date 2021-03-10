using Gang.Commands;
using Gang.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Gang.State.Commands
{
    public sealed class GangCommandExecutor<TStateData> :
        IGangCommandExecutor<TStateData>
        where TStateData : class
    {
        readonly IImmutableDictionary<string, GangCommandHandler<TStateData>> _handlers;
        readonly IGangSerializationService _serialization;

        public GangCommandExecutor(
            IGangSerializationService serialization,
            IEnumerable<GangCommandHandler<TStateData>> handlers = null
            )
        {
            _handlers = handlers
                ?.ToImmutableDictionary(h => h.DataType.GetCommandTypeName())
                ?? ImmutableDictionary<string, GangCommandHandler<TStateData>>.Empty;
            _serialization = serialization;
        }

        IGangCommand IGangCommandExecutor<TStateData>.Deserialize(
            byte[] bytes, GangAudit audit
            )
        {
            var wrapper = _serialization.Deserialize<GangCommandWrapper>(bytes);

            if (!_handlers.ContainsKey(wrapper.Type))
                throw new GangCommandHandlerNotFoundExcetion();

            var type = _handlers[wrapper.Type].DataType;

            return GangCommand.From(
                _serialization.Map(wrapper.Data, type),
                audit);
        }

        async Task<GangState<TStateData>> IGangCommandExecutor<TStateData>
            .ExecuteAsync(GangState<TStateData> state, IGangCommand command)
        {
            var dataTypeName = command.GetTypeName();

            if (!_handlers.ContainsKey(dataTypeName))
                throw new GangCommandHandlerNotFoundExcetion();

            var handler = _handlers[dataTypeName];

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