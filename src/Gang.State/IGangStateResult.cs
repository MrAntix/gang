using System.Collections.Immutable;

namespace Gang.State
{
    public interface IGangStateResult
    {
        object Command { get; }
        IImmutableList<byte[]> SendToMemberIds { get; }
    }
}