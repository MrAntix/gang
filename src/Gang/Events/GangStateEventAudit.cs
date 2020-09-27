using System;

namespace Gang.Events
{
    public class GangStateEventAudit
    {
        public GangStateEventAudit(
            byte[] memberId,
            uint sequenceNumber,
            DateTimeOffset on)
        {
            MemberId = memberId;
            SequenceNumber = sequenceNumber;
            On = on;
        }

        public byte[] MemberId { get; }
        public uint SequenceNumber { get; }
        public DateTimeOffset On { get; }
    }
}
