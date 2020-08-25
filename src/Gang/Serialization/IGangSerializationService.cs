using System;

namespace Gang.Serialization
{
    public interface IGangSerializationService
    {
        byte[] Serialize(object value);
        object Deserialize(byte[] value, Type type);
        TObject Map<TObject>(object json);
    }
}
