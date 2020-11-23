using System;

namespace Gang.Events
{
    [Serializable]
    public sealed class GangEventSequenceException :
        Exception
    {
        public GangEventSequenceException(
            uint currentSequenceNumber, uint? @eventSequenceNumber)
        {
            CurrentSequenceNumber = currentSequenceNumber;
            EventSequenceNumber = eventSequenceNumber;
        }

        public uint CurrentSequenceNumber { get; }
        public uint? EventSequenceNumber { get; }
    }
}
