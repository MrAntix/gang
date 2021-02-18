using System;

namespace Gang.Storage
{
    [Serializable]
    public class GangStoreException :
        Exception
    {
        public GangStoreException(string message) : base(message) { }
        public GangStoreException(string message, Exception inner) : base(message, inner) { }
        protected GangStoreException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
