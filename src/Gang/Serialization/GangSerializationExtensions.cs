using Gang.Commands;
using System;
using System.Linq;

namespace Gang.Serialization
{
    public static class GangSerializationServiceExtensions
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

        public static byte[] SerializeCommand(
            this IGangSerializationService service,
            uint sequenceNumber,
            string type, object data,
            uint? replySequenceNumber = null)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            return BitConverter.GetBytes(sequenceNumber)
                    .Concat(service.Serialize(new GangCommandWrapper(
                        type,
                        data,
                        replySequenceNumber
                    )))
                    .ToArray();
        }

        public static byte[] SerializeCommand(
            this IGangSerializationService service,
            uint sequenceNumber,
            object data,
            uint? replySequenceNumber = null)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            return service.SerializeCommand(
                  sequenceNumber,
                  data.GetType().GetCommandTypeName(), data,
                  replySequenceNumber
                  );
        }
    }
}
