using System;

namespace Gang
{
    public class GangAudit
    {
        public GangAudit(
            string gangId,
            uint? sequenceNumber = null,
            byte[] memberId = null,
            string userId = null,
            DateTimeOffset? on = null)
        {
            GangId = gangId;
            SequenceNumber = sequenceNumber;
            MemberId = memberId;
            UserId = userId;
            On = on ?? DateTimeOffset.UtcNow;
        }

        public string GangId { get; }
        public uint? SequenceNumber { get; }
        public byte[] MemberId { get; }
        public string UserId { get; }
        public DateTimeOffset On { get; }

        public GangAudit SetSequenceNumber(uint? sequenceNumber)
        {
            return new GangAudit(
                GangId,
                sequenceNumber,
                MemberId, UserId,
                On);
        }
    }
}
