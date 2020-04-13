using System;

namespace Gang
{
    public class GangMessageAudit
    {
        public GangMessageAudit(
            byte[] memberId,
            DateTimeOffset? on = null)
        {
            MemberId = memberId;
            On = on ?? DateTimeOffset.UtcNow;
        }

        public byte[] MemberId { get; }
        public DateTimeOffset On { get; }
    }
}
