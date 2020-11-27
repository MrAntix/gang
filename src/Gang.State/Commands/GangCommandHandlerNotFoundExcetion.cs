using System;
using System.Runtime.Serialization;

namespace Gang.State.Commands
{
    [Serializable]
    public class GangCommandHandlerNotFoundExcetion : Exception
    {
        public GangCommandHandlerNotFoundExcetion()
        {
        }

        protected GangCommandHandlerNotFoundExcetion(
            SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }
    }
}