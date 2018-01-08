namespace Gang.Serialization
{
    public static class SerializationServiceExtensions
    {
        public static T Deserialize<T>(
            this ISerializationService service, string value)
        {
            return (T) service.Deserialize(value, typeof(T));
        }
    }
}
