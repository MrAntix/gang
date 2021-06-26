using Gang.State.Commands;
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
            IEnumerable<IGangStateResult> results = null,
            IEnumerable<string> errors = null
            )
            where TData : class
        {
            return new GangState<TData>(
                data, version,
                uncommitted, results, errors
                );
        }

        public static GangState<TData> Update<TData>(
            GangState<TData> state,
            TData data = null,
            uint? version = null,
            IEnumerable<object> uncommitted = null,
            IEnumerable<IGangStateResult> results = null,
            IEnumerable<string> errors = null
            )
            where TData : class
        {
            return Create(
                data ?? state.Data,
                version ?? state.Version,
                uncommitted ?? state.Uncommitted,
                results ?? state.Results,
                errors ?? state.Errors
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
            IEnumerable<IGangStateResult> results = null,
            IEnumerable<string> errors = null
            )
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Version = version;
            Uncommitted = uncommitted.ToImmutableListDefaultEmpty();
            Results = results.ToImmutableListDefaultEmpty();
            Errors = errors?.ToImmutableList();
            HasErrors = Errors?.Any() ?? false;
        }

        public TData Data { get; }
        public uint Version { get; }
        public IImmutableList<object> Uncommitted { get; }
        public IImmutableList<IGangStateResult> Results { get; }
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
                : GangState.Update(
                    this,
                    data: apply(eventData),
                    version: Version + 1,
                    uncommitted: Uncommitted.Append(eventData)
                    );
        }

        public GangState<TData> RaiseErrors(
            IEnumerable<string> errors)
        {
            return GangState.Update(
                this,
                errors: Errors?.Concat(errors) ?? errors
                );
        }

        public GangState<TData> RaiseErrors(
            params string[] errors)
        {
            return RaiseErrors(errors as IEnumerable<string>);
        }

        public GangState<TData> AddResult(
            IEnumerable<byte[]> sendToMemberId,
            object command
            )
        {
            return GangState.Update(
                this,
                results: Results.Add(
                        GangStateResult.From(
                            sendToMemberId,
                            command
                            )
                    )
                );
        }

        public GangState<TData> AddResult(
            byte[] sendToMemberId,
            object command
            )
        {
            return AddResult(
                new[] { sendToMemberId },
                command
                );
        }

        /// <summary>
        /// Get errors or notifications on state,
        /// if there are errors only they will be returned, state has not changed
        /// </summary>
        /// <param name="audit">Audit</param>
        /// <returns>List of notification command messages</returns>
        public IImmutableList<IGangStateResult> GetResults(GangAudit audit)
        {
            if (HasErrors)
                return Errors
                    .Select(text => GangStateResult.From(
                            new[] { audit.MemberId },
                            new GangNotify(text, type: GangNotificationTypes.Danger)
                            )
                    )
                    .ToImmutableList();

            return Results;
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
