using Gang.Management;
using Gang.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gang.Commands
{
    public static class GangCommandExtensions
    {
        public static string GetCommandTypeName(
            this Type type)
        {
            return type.Name
                .TryDecapitalize()
                .TryTrimEnd("Command");
        }

        public static Task SendCommandAsync(
            this IGangController controller,
            object data,
            IEnumerable<byte[]> memberIds = null,
            uint? replySequenceNumber = null)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            return controller.SendCommandAsync(
                data.GetType().GetCommandTypeName(), data,
                memberIds, replySequenceNumber);
        }

        public static byte[] SerializeCommandData(
            this IGangSerializationService service,
            string type, object data,
            uint? replySequenceNumber = null)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            return service.Serialize(new GangCommandWrapper(
                        type,
                        data,
                        replySequenceNumber
                    ));
        }

        public static byte[] SerializeCommandData(
            this IGangSerializationService service,
            object data,
            uint? replySequenceNumber = null)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            return service.SerializeCommandData(
                        data.GetType().GetCommandTypeName(), data,
                        replySequenceNumber
                    );
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
                    .Concat(service.SerializeCommandData(
                        type,
                        data,
                        replySequenceNumber
                    ))
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