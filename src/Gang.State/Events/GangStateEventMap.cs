using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gang.State.Events
{
    public sealed class GangStateEventMap
    {
        readonly ImmutableDictionary<string, Type> _map;

        public GangStateEventMap(
            IEnumerable<KeyValuePair<string, Type>> map = null
            )
        {
            _map = map
                ?.ToImmutableDictionary()
                ?? ImmutableDictionary<string, Type>.Empty;
        }

        public GangStateEventMap Add<TStateData>()
            where TStateData : class
        {
            return new GangStateEventMap(
                _map.AddRange(
                    GangState<TStateData>.ApplyMethods.Keys
                        .ToImmutableDictionary(GetName<TStateData>)
                ));
        }

        public Type GetType(string name)
        {
            return _map[name];
        }

        public static string GetName<TStateData>(Type dataType)
        {
            return $"{typeof(TStateData).Name}.{dataType.Name}";
        }

    }
}
