using System;

namespace Gang.Contracts
{
    public class GangAudit
    {
        public GangAudit(
            string gangId,
            byte[] memberId = null,
            uint? sequenceNumber = null,
            string userId = null,
            DateTimeOffset? on = null)
        {
            GangId = gangId;
            MemberId = memberId;
            SequenceNumber = sequenceNumber;
            UserId = userId;
            On = on ?? DateTimeOffset.UtcNow;
        }

        public string GangId { get; }
        public byte[] MemberId { get; }
        public uint? SequenceNumber { get; }
        public string UserId { get; }
        public DateTimeOffset On { get; }
    }
}
