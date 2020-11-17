using System;

namespace Gang.Management.Contracts
{
    public class GangError
    {
        public GangError(
            object eventData,
            Exception exception)
        {
            EventData = eventData;
            Exception = exception;
        }

        public object EventData { get; }
        public Exception Exception { get; }
    }
}
