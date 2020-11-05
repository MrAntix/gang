using System;

namespace Gang.Contracts
{
    public class GangAudit
    {
        public GangAudit(
            string gangId,
            byte[] memberId = null,
            uint? sequenceNumber = null,
            DateTimeOffset? on = null)
        {
            GangId = gangId;
            MemberId = memberId;
            SequenceNumber = sequenceNumber;
            On = on ?? DateTimeOffset.UtcNow;
        }

        public string GangId { get; }
        public byte[] MemberId { get; }
        public uint? SequenceNumber { get; }
        public DateTimeOffset On { get; }
    }
}
