using System;

namespace Gang.Serialization
{
    public interface IGangSerializationService
    {
        string Serialize(object value);
        object Deserialize(string value, Type type);
    }
}
