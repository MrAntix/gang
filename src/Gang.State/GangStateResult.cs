using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gang.State
{
    public static class GangStateResult
    {
        public static IGangStateResult From(
            IEnumerable<byte[]> sendToMemberId,
            object command
            )
        {
            var type = typeof(GangStateResult<>)
                .MakeGenericType(command.GetType());

            return (IGangStateResult)Activator.CreateInstance(
                type, sendToMemberId, command
                );
        }

    }

    public sealed class GangStateResult<TCommand> :
        IGangStateResult
    {
        public GangStateResult(
            IEnumerable<byte[]> sendToMemberId,
            TCommand command
            )
        {
            SendToMemberIds = sendToMemberId.ToImmutableListDefaultEmpty();
            Command = command;
        }

        public IImmutableList<byte[]> SendToMemberIds { get; }
        public TCommand Command { get; }

        object IGangStateResult.Command => Command;
    }
}