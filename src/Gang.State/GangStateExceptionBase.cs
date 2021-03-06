using System;

namespace Gang.State
{
    public abstract class GangStateExceptionBase :
        Exception
    {
        public GangStateExceptionBase(
            GangAudit audit)
        {
            Audit = audit;
        }

        public GangAudit Audit { get; }
    }
}
