using Gang.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Gang
{
    public abstract class GangStatefulHostBase<TState> :
        GangHostBase
        where TState : class
    {
        TState _state;
        uint _stateVersion;

        protected GangStatefulHostBase(
            TState initialState = null)
        {
            _state = initialState;
        }

        protected virtual Task OnStateEventAsync(
            object e, GangAudit a)
        {
            return Task.CompletedTask;
        }

        public TState State => _state;
        public uint StateVersion => _stateVersion;
        IImmutableDictionary<Type, Func<TState, GangEvent, TState>> _stateApplyMethods;

        public void SetState(
            TState state, uint version = 0
            )
        {
            _state = state;
            _stateVersion = version;
        }

        public void ApplyStateEvents(
            IEnumerable<GangEvent> events)
        {
            if (_stateApplyMethods == null)
                _stateApplyMethods = typeof(TState)
                    .GetMethods()
                    .Select(NormaliseMethod)
                    .Where(m => m != null)
                    .ToImmutableDictionary(kv => kv.Item1, kv => kv.Item2);

            _state = events.Aggregate(
                _state,
                (s, w) =>
                {
                    if (_stateVersion >= w.Audit.SequenceNumber)
                        throw new GangStateOutOfSequenceException(
                            _stateVersion, w.Audit.SequenceNumber
                            );

                    _stateVersion = w.Audit.SequenceNumber.Value;
                    return _stateApplyMethods[w.Data.GetType()](s, w);
                });
        }

        protected async Task RaiseStateEventAsync<TEvent>(
            TEvent e, byte[] memberId,
            Func<TEvent, GangAudit, TState> apply)
        {
            _stateVersion++;

            var a = new GangAudit(
                Controller.GangId,
                memberId,
                _stateVersion, DateTimeOffset.UtcNow);

            _state = apply(e, a);

            await OnStateEventAsync(e, a);
        }

        public async Task RaiseStateEventAsync<TEvent>(
            TEvent e, byte[] memberId,
            Func<TEvent, TState> apply)
        {
            await RaiseStateEventAsync(e, memberId,
                (e, _) => apply(e));
        }

        static Tuple<Type, Func<TState, GangEvent, TState>> NormaliseMethod(MethodInfo m)
        {
            if (m.ReturnType != typeof(TState)) return null;

            var p = m.GetParameters();
            if (p.Length < 1 || p.Length > 2) return null;
            if (p.Length == 2 && p[1].ParameterType != typeof(GangAudit)) return null;

            return Tuple
                .Create<Type, Func<TState, GangEvent, TState>>(
                    p[0].ParameterType,
                    (s, w) => (TState)m
                         .Invoke(s, p.Length == 1
                         ? new[] { w.Data }
                         : new[] { w.Data, w.Audit })
                );
        }
    }
}
