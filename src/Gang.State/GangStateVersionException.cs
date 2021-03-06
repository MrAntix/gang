using System;

namespace Gang.State
{
    [Serializable]
    public sealed class GangStateVersionException :
        GangStateExceptionBase
    {
        public GangStateVersionException(
            uint expectedVersion,
            GangAudit audit) : base(audit)
        {
            ExpectedVersion = expectedVersion;
        }

        public uint ExpectedVersion { get; }
    }
}
