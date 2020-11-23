using Gang.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gang.Management
{
    public static class GangManagmentExtensions
    {
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

        public static IGangMember GetMember(
            this IGangController controller,
            byte[] memberId
            )
        {
            return controller.GetGang().MemberById(memberId);
        }

        public static Task DisconnectAsync(
            this IGangController controller,
            string memberId,
            string reason = null)
        {
            return controller.DisconnectAsync(memberId.GangToBytes(), reason);
        }
    }
}
