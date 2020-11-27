using Gang.State.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Gang.State
{
    public sealed class GangState<TData>
        where TData : class, new()
    {
        public GangState(
            TData data = null,
            uint version = 0,
            IEnumerable<object> uncommitted = null,
            IEnumerable<string> errors = null
            )
        {
            Data = data ?? new TData();
            Version = version;
            Uncommitted = uncommitted
                ?.ToImmutableList()
                ?? ImmutableList<object>.Empty;
            Errors = errors
                ?.ToImmutableList();
        }

        public TData Data { get; }
        public uint Version { get; }
        public IImmutableList<object> Uncommitted { get; }
        public IImmutableList<string> Errors { get; }

        public GangState<TData> RaiseEvent<TEventData>(
            TEventData eventData,
            Func<TEventData, TData> apply)
        {
            return new GangState<TData>(
                    apply(eventData),
                    Version + 1,
                    Uncommitted.Append(eventData),
                    Errors
                    );
        }

        public GangState<TData> RaiseErrors(
            IEnumerable<string> errors)
        {
            return new GangState<TData>(
                Data, Version, Uncommitted,
                errors
                );
        }

        public GangState<TData> RaiseErrors(
            params string[] errors)
        {
            return RaiseErrors(errors as IEnumerable<string>);
        }

        public GangState<TData> Apply(
            IEnumerable<IGangEvent> events)
        {
            var version = Version;
            var data = events.Aggregate(
                Data,
                (s, e) =>
                {
                    version++;
                    if (version != e.Audit.SequenceNumber)
                        throw new GangEventSequenceException(
                            version, e.Audit.SequenceNumber
                            );

                    return _applyMethods[e.Data.GetType()](s, e.Data);
                });

            return new GangState<TData>(
                    data, version, null,
                    null
                    );
        }

        static GangState()
        {
            _applyMethods = typeof(TData)
                    .GetMethods()
                    .Select(WrapMethod)
                    .Where(m => m != null)
                    .ToImmutableDictionary(kv => kv.Item1, kv => kv.Item2);
        }

        static readonly ImmutableDictionary<Type, Func<TData, object, TData>> _applyMethods;

        static Tuple<Type, Func<TData, object, TData>> WrapMethod(
            MethodInfo method)
        {
            if (method.ReturnType != typeof(TData)) return null;

            var p = method.GetParameters();
            if (p.Length != 1) return null;

            return Tuple
                .Create<Type, Func<TData, object, TData>>(
                    p[0].ParameterType,
                    (state, data) => (TData)method
                         .Invoke(state, new[] { data })
                );
        }
    }
}
