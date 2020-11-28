using Gang.State.Events;
using System;
using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Gang.State.Storage
{
    public sealed class InMemoryGangStateStore :
        IGangStateStore
    {
        readonly ReplaySubject<IGangEvent> _events;
        uint _sequenceNumber;

        IImmutableDictionary<string, object> _states;

        public InMemoryGangStateStore()
        {
            _events = new ReplaySubject<IGangEvent>();
            _states = ImmutableDictionary<string, object>.Empty;
        }

        Task<GangState<TStateData>> IGangStateStore
            .CommitAsync<TStateData>(string gangId, GangState<TStateData> state, GangAudit audit)
        {
            foreach (var eventData in state.Uncommitted)
                _events.OnNext(GangEvent.From(eventData, audit, ++_sequenceNumber));

            state = new GangState<TStateData>(state.Data, state.Version);

            _states = _states
                .SetItem(gangId, state);

            return Task.FromResult(state);
        }

        Task<GangState<TStateData>> IGangStateStore
            .RestoreAsync<TStateData>(string gangId)
        {
            return Task.FromResult(
                    _states[gangId] as GangState<TStateData>
                );
        }

        IDisposable IGangStateStore
            .Subscribe(Func<IGangEvent, Task> observer, uint? startSequenceNumber)
        {
            if (!startSequenceNumber.HasValue)
                startSequenceNumber = _sequenceNumber;

            return _events
                .Skip((int)startSequenceNumber)
                .Subscribe(e => observer(e));
        }
    }
}
