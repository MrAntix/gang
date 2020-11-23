using Gang.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Gang.State
{
    public class GangState<TState>
    {
        IImmutableDictionary<Type, Func<TState, IGangEvent, TState>> _stateApplyMethods;

        public GangState(
            TState data, uint version)
        {
            Data = data;
            Version = version;
        }

        public TState Data { get;set;  }
        public uint Version { get; set; }

        
        public void ApplyStateEvents(
            IEnumerable<IGangEvent> events)
        {
            if (_stateApplyMethods == null)
                _stateApplyMethods = typeof(TState)
                    .GetMethods()
                    .Select(NormaliseMethod)
                    .Where(m => m != null)
                    .ToImmutableDictionary(kv => kv.Item1, kv => kv.Item2);

            Data = events.Aggregate(
                Data,
                (s, w) =>
                {
                    if (Version >= w.Audit.SequenceNumber)
                        throw new GangEventSequenceException(
                            Version, w.Audit.SequenceNumber
                            );

                    Version = w.Audit.SequenceNumber.Value;
                    return _stateApplyMethods[w.Data.GetType()](s, w);
                });
        }

        protected void RaiseEvent<TEventData>(
            TEventData e, GangAudit a,
            Func<TEventData, GangAudit, TState> apply)
        {
            Version++;

            a = new GangAudit(
                a.GangId,
                a.MemberId,
                Version,
                a.UserId
                );

            Data = apply(e, a);
        }

        public void RaiseEvent<TEventData>(
            TEventData e, GangAudit a,
            Func<TEventData, TState> apply)
        {
            RaiseEvent(e, a,
                (e, _) => apply(e));
        }


        static Tuple<Type, Func<TState, IGangEvent, TState>> NormaliseMethod(MethodInfo m)
        {
            if (m.ReturnType != typeof(TState)) return null;

            var p = m.GetParameters();
            if (p.Length < 1 || p.Length > 2) return null;
            if (p.Length == 2 && p[1].ParameterType != typeof(GangAudit)) return null;

            return Tuple
                .Create<Type, Func<TState, IGangEvent, TState>>(
                    p[0].ParameterType,
                    (s, w) => (TState)m
                         .Invoke(s, p.Length == 1
                         ? new[] { w.Data }
                         : new[] { w.Data, w.Audit })
                );
        }
    }
}
