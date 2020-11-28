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
            uint? replySequence = null)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            return controller.SendCommandAsync(
                data.GetType().GetCommandTypeName(), data,
                memberIds, replySequence);
        }

        public static byte[] SerializeCommandData(
            this IGangSerializationService service,
            string type, object data,
            uint? replySequence = null)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            return service.Serialize(new GangCommandWrapper(
                        type,
                        data,
                        replySequence
                    ));
        }

        public static byte[] SerializeCommandData(
            this IGangSerializationService service,
            object data,
            uint? replySequence = null)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            return service.SerializeCommandData(
                        data.GetType().GetCommandTypeName(), data,
                        replySequence
                    );
        }

        public static byte[] SerializeCommand(
            this IGangSerializationService service,
            uint sequence,
            string type, object data,
            uint? replySequence = null)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            return BitConverter.GetBytes(sequence)
                    .Concat(service.SerializeCommandData(
                        type,
                        data,
                        replySequence
                    ))
                    .ToArray();
        }

        public static byte[] SerializeCommand(
            this IGangSerializationService service,
            uint sequenceNumber,
            object data,
            uint? replySequence = null)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            return service.SerializeCommand(
                  sequenceNumber,
                  data.GetType().GetCommandTypeName(), data,
                  replySequence
                  );
        }
    }
}