using System;

namespace Gang.Serialization
{
    public interface IGangSerializationService
    {
        byte[] Serialize(object value);
        object Deserialize(byte[] value, Type type);
        object Map(object value, Type type);
    }
}
