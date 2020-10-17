using System;

namespace Gang
{
    public class GangMessageAudit
    {
        public GangMessageAudit(
            byte[] gangId,
            byte[] memberId = null,
            uint? sequenceNumber = null,
            DateTimeOffset? on = null)
        {
            GangId = gangId;
            MemberId = memberId;
            SequenceNumber = sequenceNumber;
            On = on ?? DateTimeOffset.UtcNow;
        }

        public byte[] GangId { get; }
        public byte[] MemberId { get; }
        public uint? SequenceNumber { get; }
        public DateTimeOffset On { get; }
    }
}
