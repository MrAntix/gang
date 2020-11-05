namespace Gang.Commands
{
    public interface IGangCommandWrapper
    {
        object Command { get; }
        uint? InReplyToSequenceNumber { get; }
        string Type { get; }
    }
}