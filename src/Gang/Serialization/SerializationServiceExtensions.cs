namespace Gang.Serialization
{
    public static class SerializationServiceExtensions
    {
        public static T Deserialize<T>(
            this IGangSerializationService service, string value)
        {
            return (T)service.Deserialize(value, typeof(T));
        }
    }
}
