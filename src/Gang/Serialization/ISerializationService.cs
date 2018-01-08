using System;

namespace Gang.Serialization
{
    public interface ISerializationService
    {
        string Serialize(object value);
        object Deserialize(string value, Type type);
    }
}
