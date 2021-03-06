using System;

namespace Gang.State
{
    [Serializable]
    public sealed class GangStateCommandException :
        GangStateExceptionBase
    {
        public GangStateCommandException(
            object commandData,
            GangAudit audit) : base(audit)
        {
            Type = commandData?.GetType().Name ?? "Unknown";
            CommandData = commandData;
        }

        public string Type { get; }

        public object CommandData { get; }
    }
}
