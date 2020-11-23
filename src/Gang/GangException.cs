using System;

namespace Gang
{
    [Serializable]
    public class GangException : Exception
    {
        public GangException(string message) : base(message) { }
        public GangException(string message, Exception inner) : base(message, inner) { }
        protected GangException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
