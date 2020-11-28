using System;

namespace Gang.State
{
    [Serializable]
    public sealed class GangStateVersionException :
        Exception
    {
        public GangStateVersionException(
            uint expectedVersion,
            GangAudit audit)
        {
            ExpectedVersion = expectedVersion;
            Audit = audit;
        }

        public uint ExpectedVersion { get; }
        public GangAudit Audit { get; }
    }
}
