using Gang.State.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Gang.State
{
    public static class GangState
    {
        public static GangState<TData> Create<TData>(
             TData data,
            uint version = 0,
            IEnumerable<object> uncommitted = null,
            IEnumerable<string> errors = null
            )
            where TData : class
        {
            return new GangState<TData>(
                data, version,
                uncommitted, errors
                );
        }
    }

    public sealed class GangState<TData>
        where TData : class
    {

        public GangState(
            TData data,
            uint version = 0,
            IEnumerable<object> uncommitted = null,
            IEnumerable<string> errors = null
            )
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Version = version;
            Uncommitted = uncommitted.ToImmutableListDefaultEmpty();
            Errors = errors?.ToImmutableList();
            HasErrors = Errors?.Any() ?? false;
        }

        public TData Data { get; }
        public uint Version { get; }
        public IImmutableList<object> Uncommitted { get; }
        public IImmutableList<string> Errors { get; }
        public bool HasErrors { get; }

        public GangState<TData> Assert(
            string nullOrFailMessage,
            string overrideFailMessage = null)
        {
            return nullOrFailMessage == null
                ? this
                : RaiseErrors(overrideFailMessage ?? nullOrFailMessage);
        }

        public GangState<TData> Assert(
            bool assertion,
            string failMessage = null)
        {
            return assertion
                ? this
                : RaiseErrors(failMessage);
        }

        public GangState<TData> RaiseEvent<TEventData>(
            TEventData eventData,
            Func<TEventData, TData> apply)
        {
            return Errors?.Any() ?? false
                ? this
                : GangState.Create(
                    apply(eventData),
                    Version + 1,
                    Uncommitted.Append(eventData),
                    Errors
                    );
        }

        public GangState<TData> RaiseErrors(
            IEnumerable<string> errors)
        {
            return GangState.Create(
                Data, Version, Uncommitted,
                Errors?.Concat(errors) ?? errors
                );
        }

        public GangState<TData> RaiseErrors(
            params string[] errors)
        {
            return RaiseErrors(errors as IEnumerable<string>);
        }

        public GangState<TData> Apply(
            IEnumerable<IGangStateEvent> events)
        {
            var version = Version;
            var data = events.Aggregate(
                Data,
                (s, e) =>
                {
                    version++;
                    if (version != e.Audit.Version)
                        throw new GangStateVersionException(version, e.Audit);

                    var method = ApplyMethods[e.Data.GetType()];

                    return method(s, e.Data);
                });

            return GangState.Create(
                    data, version, null,
                    null
                    );
        }

        public static readonly ImmutableDictionary<Type, Func<TData, object, TData>> ApplyMethods
            = typeof(TData)
                .GetMethods()
                .Select(WrapMethod)
                .Where(m => m.Key != null)
                .ToImmutableDictionary();

        static KeyValuePair<Type, Func<TData, object, TData>> WrapMethod(
            MethodInfo method
            )
        {
            if (method.ReturnType != typeof(TData)) return default;

            var p = method.GetParameters();
            if (p.Length != 1) return default;

            return KeyValuePair
                .Create<Type, Func<TData, object, TData>>(
                    p[0].ParameterType,
                    (state, data) => (TData)method.Invoke(state, new[] { data })
                );
        }

        public static IImmutableList<IGangStateEvent> EventSequenceFrom(
            IEnumerable<object> data,
            GangAudit audit
        )
        {
            var sequence = audit.Version ?? 0U;
            return data
                .Select((d, i) =>
                    GangStateEvent.From(d, audit.SetVersion(++sequence)
                    )
                )
                .ToImmutableList();
        }
    }
}
