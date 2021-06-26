using Gang.Management;
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
        const string STORE_EVENTS = "events";
        const string STORE_CACHE = "cache";
        const string KEY_SEQUENCE_NUMBER = ".sn";

        readonly IGangSerializationService _serializer;
        readonly GangStateEventMap _eventMap;
        readonly IGangManager _gangManager;
        readonly Subject<IGangStateEvent> _events;
        readonly IGangStore<GangStateEventWrapper> _eventStore;
        readonly IGangStore<uint> _sequenceNumberStore;
        readonly IGangStore<object> _cache;

        uint _sequenceNumber;

        public GangStateStore(
            IGangSerializationService serializer,
            GangStateEventMap eventMap,
            IGangStoreFactory storeFactory,
            IGangManager gangManager
            )
        {
            _events = new Subject<IGangStateEvent>();
            _eventStore = storeFactory
                .For<GangStateEventWrapper>()
                .AddIndex(e => e.Audit.GangId)
                .Create(STORE_EVENTS);
            _sequenceNumberStore = storeFactory
                .For<uint>()
                .Create(STORE_EVENTS);

            _cache = storeFactory
                .For<object>()
                .Create(STORE_CACHE);
            _serializer = serializer;
            _eventMap = eventMap;
            _gangManager = gangManager;
            _sequenceNumber = _sequenceNumberStore
                .TryGetAsync(KEY_SEQUENCE_NUMBER)
                .GetAwaiter().GetResult();
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

                var key = GetEventKey(++_sequenceNumber);

                await _eventStore.SetAsync(
                    key, wrapper,
                    overwrite: false
                    );
            }

            await _cache.SetAsync(gangId,
                new { state.Data, state.Version }
                );
            await _sequenceNumberStore.SetAsync(KEY_SEQUENCE_NUMBER, _sequenceNumber);

            return GangState.Create(state.Data, state.Version);
        }

        async Task<GangState<TStateData>> IGangStateStore
            .RestoreAsync<TStateData>(
                string gangId, TStateData initial
            )
            where TStateData : class
        {
            var data = await _cache.TryGetAsync(gangId);
            var state = _serializer.Map<GangState<TStateData>>(data);

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
            var progress = GangProgress.Start(
                _gangManager, gangId,
                "Loading", keys.Count
                );

            foreach (var key in keys)
            {
                var e = await GetEventAsync(key);

                if (++version != e.Audit.Version)
                    throw new GangStateVersionException(version, e.Audit);

                var method = GangState<TStateData>.ApplyMethods[e.Data.GetType()];
                data = method(data, e.Data);

                progress.Increment(1);
            }

            progress.End();

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

        static string GetEventKey(uint sequenceNumber)
        {
            return $"{sequenceNumber++:000000000}";
        }
    }
}
