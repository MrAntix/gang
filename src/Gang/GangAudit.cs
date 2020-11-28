using System;

namespace Gang
{
    public sealed class GangAudit
    {
        public GangAudit(
            string gangId,
            uint? sequence = null,
            byte[] memberId = null,
            string userId = null,
            DateTimeOffset? on = null)
        {
            GangId = gangId;
            Sequence = sequence;
            MemberId = memberId;
            UserId = userId;
            On = on ?? DateTimeOffset.UtcNow;
        }

        public string GangId { get; }
        public uint? Sequence { get; }
        public byte[] MemberId { get; }
        public string UserId { get; }
        public DateTimeOffset On { get; }

        public GangAudit SetSequence(uint sequence)
        {
            return new GangAudit(
                GangId,
                sequence,
                MemberId, UserId,
                On);
        }
    }
}
