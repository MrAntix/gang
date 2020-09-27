using System;

namespace Gang
{
    [Serializable]
    public sealed class GangStateOutOfSequenceException :
        Exception
    {
        public GangStateOutOfSequenceException(
            uint currentSequenceNumber, uint @eventSequenceNumber)
        {
            CurrentSequenceNumber = currentSequenceNumber;
            EventSequenceNumber = eventSequenceNumber;
        }

        public uint CurrentSequenceNumber { get; }
        public uint EventSequenceNumber { get; }
    }
}
