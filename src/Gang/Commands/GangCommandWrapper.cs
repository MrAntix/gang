namespace Gang.Commands
{
    public sealed class GangCommandWrapper
    {
        public GangCommandWrapper(
            string type,
            object command,
            uint? rsn)
        {
            Type = type;
            Command = command;
            InReplyToSequenceNumber = rsn;
        }

        public string Type { get; }
        public object Command { get; }
        public uint? InReplyToSequenceNumber { get; }
    }
}