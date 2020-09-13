using System;

namespace Gang
{
    public class GangMessageAudit
    {
        public GangMessageAudit(
            byte[] memberId,
            short? sequenceNumber = null,
            DateTimeOffset? on = null)
        {
            MemberId = memberId;
            SequenceNumber = sequenceNumber;
            On = on ?? DateTimeOffset.UtcNow;
        }

        public byte[] MemberId { get; }
        public short? SequenceNumber { get; }
        public DateTimeOffset On { get; }
    }
}
