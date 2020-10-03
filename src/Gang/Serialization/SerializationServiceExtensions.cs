namespace Gang.Serialization
{
    public static class SerializationServiceExtensions
    {
        public static T Deserialize<T>(
            this IGangSerializationService service, byte[] value)
        {
            return (T)service.Deserialize(value, typeof(T));
        }

        public static TObject Map<TObject>(
            this IGangSerializationService service, object value)
        {
            return (TObject)service.Map(value, typeof(TObject));
        }
    }
}
