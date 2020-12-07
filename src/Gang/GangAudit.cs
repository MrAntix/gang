using System;

namespace Gang
{
    public sealed class GangAudit
    {
        public GangAudit(
            string gangId,
            uint? version = null,
            byte[] memberId = null,
            string userId = null,
            DateTimeOffset? on = null)
        {
            GangId = gangId;
            Version = version;
            MemberId = memberId;
            UserId = userId;
            On = on ?? DateTimeOffset.UtcNow;
        }

        public string GangId { get; }
        public uint? Version { get; }
        public byte[] MemberId { get; }
        public string UserId { get; }
        public DateTimeOffset On { get; }

        public GangAudit SetVersion(uint value)
        {
            return new GangAudit(
                GangId,
                value,
                MemberId, UserId,
                On);
        }
    }
}
