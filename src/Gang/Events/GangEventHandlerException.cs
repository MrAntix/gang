using System;
using System.Reflection;

namespace Gang.Events
{
    [Serializable]
    public class GangEventHandlerException<TDataImplements> :
        Exception
    {
        public GangEventHandlerException(
            TDataImplements attempted,
            TargetInvocationException inner) :
            base(inner.InnerException.Message, inner)
        {
            Attempted = attempted;
        }

        protected GangEventHandlerException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) :
            base(info, context)
        { }

        public TDataImplements Attempted { get; }
    }
}
