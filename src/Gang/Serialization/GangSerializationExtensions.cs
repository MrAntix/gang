using System;
using System.Text;

namespace Gang.Serialization
{
    public static class GangSerialization
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

        public static ReadOnlySpan<byte> Base64UrlToBytes(
            string value
            )
        {
            value = value.Replace('-', '+');
            value = value.Replace('_', '/');

            value += "==="[((value.Length + 3) % 4)..];

            return new ReadOnlySpan<byte>(
                Convert.FromBase64String(value)
                );
        }

        public static string Base64UrlToString(
            string value
            )
        {
            return Encoding.UTF8.GetString(
                Base64UrlToBytes(value).ToArray()
            );
        }

        public static string BytesToBase64Url(ReadOnlySpan<byte> value)
        {
            var s = Convert.ToBase64String(value.ToArray());

            s = s.Split('=')[0];
            s = s.Replace('+', '-');
            s = s.Replace('/', '_');

            return s;
        }

        public static string StringToBase64Url(
            string value)
        {
            return BytesToBase64Url(
                Encoding.UTF8.GetBytes(value)
                );
        }
    }
}
