using Gang.State.Commands;
using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Gang.State
{
    [Serializable]
    internal class GangCommandHandlerException :
        Exception
    {
        public GangCommandHandlerException(
            IGangCommand command, TargetInvocationException tiex)
            : base("Command failed", tiex)
        {
            Command = command;
        }

        public IGangCommand Command { get; }

        protected GangCommandHandlerException(
            SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }
    }
}