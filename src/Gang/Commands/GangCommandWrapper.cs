namespace Gang.Commands
{
    public sealed class GangCommandWrapper
    {
        public GangCommandWrapper(
            string type,
            object data,
            uint? rsn)
        {
            Type = type;
            Data = data;
            InReplyToSequenceNumber = rsn;
        }

        public string Type { get; }
        public object Data { get; }
        public uint? InReplyToSequenceNumber { get; }
    }
}