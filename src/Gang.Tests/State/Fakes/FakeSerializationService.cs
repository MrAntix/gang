using Gang.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Gang.Tests.State.Fakes
{
    public sealed class FakeSerializationService :
        IGangSerializationService
    {
        readonly IImmutableDictionary<Type, object> _deserialize;

        public FakeSerializationService(
            IEnumerable<object> deserialize = null)
        {
            _deserialize = deserialize
                ?.ToImmutableDictionary(d => d.GetType())
                ?? ImmutableDictionary<Type, object>.Empty;
        }

        public object Deserialize(byte[] value, Type type)
        {
            DeserializeCalls = DeserializeCalls.Add(new DeserializeCall(value, type));

            return _deserialize[type];
        }
        public IImmutableList<DeserializeCall> DeserializeCalls { get; private set; } = ImmutableList<DeserializeCall>.Empty;
        public sealed class DeserializeCall
        {
            public DeserializeCall(
                byte[] value, Type type)
            {
                Value = value;
                Type = type;
            }

            public byte[] Value { get; }
            public Type Type { get; }
        }
        public FakeSerializationService SetupDeserialize(
            object o)
        {
            return new FakeSerializationService(
                _deserialize.Values.Append(o)
                );
        }


        public object Map(object value, Type type)
        {
            MapCalls = MapCalls.Add(new MapCall(value, type));

            return value;
        }
        public IImmutableList<MapCall> MapCalls { get; private set; } = ImmutableList<MapCall>.Empty;
        public sealed class MapCall
        {
            public MapCall(
                object value, Type type)
            {
                Value = value;
                Type = type;
            }

            public object Value { get; }
            public Type Type { get; }
        }

        public byte[] Serialize(object value)
        {
            SerializeCalls = SerializeCalls.Add(new SerializeCall(value));

            return Array.Empty<byte>();
        }
        public IImmutableList<SerializeCall> SerializeCalls { get; private set; } = ImmutableList<SerializeCall>.Empty;
        public sealed class SerializeCall
        {
            public SerializeCall(
                object value)
            {
                Value = value;
            }

            public object Value { get; }
        }
    }
}
