using Gang.Serialization;
using Gang.State.Events;
using Gang.Storage;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Gang.State.Storage
{
    public sealed class GangStateStore :
        IGangStateStore
    {
        readonly IGangSerializationService _serializer;
        readonly GangStateEventMap _eventMap;
        readonly Subject<IGangStateEvent> _events;
        readonly IGangStore<GangStateEventWrapper> _eventStore;

        readonly IGangStore<object> _cache;

        uint _sequenceNumber;

        public GangStateStore(
            IGangSerializationService serializer,
            GangStateEventMap eventMap,
            IGangStoreFactory storeFactory
            )
        {
            _events = new Subject<IGangStateEvent>();
            _eventStore = storeFactory
                .For<GangStateEventWrapper>()
                .AddIndex(e => e.Audit.GangId)
                .Create("events");

            _cache = storeFactory
                .For<object>()
                .Create("cache");
            _serializer = serializer;
            _eventMap = eventMap;
        }

        async Task<GangState<TStateData>> IGangStateStore
            .CommitAsync<TStateData>(string gangId, GangState<TStateData> state, GangAudit audit)
        {
            var version = state.Version - (uint)state.Uncommitted.Count;
            foreach (var eventData in state.Uncommitted)
            {
                var e = GangStateEvent.From(eventData, audit);
                var wrapper = new GangStateEventWrapper(
                    eventData,
                    GangStateEventMap.GetName<TStateData>(eventData.GetType()),
                    audit.SetVersion(++version)
                    );

                _events.OnNext(e);

                await _eventStore.PutAsync(GetEventKey(++_sequenceNumber), wrapper);
            }

            await _cache.PutAsync(gangId, state);

            return GangState.Create(state.Data, state.Version);
        }

        async Task<GangState<TStateData>> IGangStateStore
            .RestoreAsync<TStateData>(
                string gangId, TStateData initial
            )
            where TStateData : class
        {
            var state = await _cache.TryGetAsync(gangId) as GangState<TStateData>;

            return state ?? await RehydrateAsync(gangId, initial);
        }

        async Task<GangState<TStateData>> RehydrateAsync<TStateData>(
            string gangId, TStateData initial
        )
            where TStateData : class
        {
            var data = initial;
            var version = 0U;

            var keys = await _eventStore.TryGetIndexedKeys(gangId);
            foreach (var key in keys)
            {
                _sequenceNumber++;
                var e = await GetEventAsync(key);

                if (++version != e.Audit.Version)
                    throw new GangStateVersionException(version, e.Audit);

                var method = GangState<TStateData>.ApplyMethods[e.Data.GetType()];
                data = method(data, e.Data);
            }

            return GangState.Create(data, version);
        }

        IDisposable IGangStateStore
            .Subscribe(Func<IGangStateEvent, Task> observer, uint? startSequenceNumber)
        {
            var sequenceNumber = startSequenceNumber.HasValue
                ? startSequenceNumber.Value
                : _sequenceNumber;

            while (sequenceNumber < _sequenceNumber)
            {
                var key = GetEventKey(++sequenceNumber);
                var e = GetEventAsync(key).GetAwaiter().GetResult();

                observer(e);
            }

            return _events
                .Subscribe(e => observer(e));
        }

        async Task<IGangStateEvent> GetEventAsync(string key)
        {
            var wrapper = await _eventStore.TryGetAsync(key);
            var type = _eventMap.GetType(wrapper.Type);

            return GangStateEvent.From(
                _serializer.Map(wrapper.Data, type),
                wrapper.Audit
                );
        }

        static string GetEventKey(uint sequenceNumber) => $"{sequenceNumber++:000000000}";
    }
}
