namespace Gang.Commands
{
    public sealed class GangCommandWrapper
    {
        public GangCommandWrapper(
            string type,
            object data,
            uint? rsn = null
            )
        {
            Type = type;
            Data = data;
            Rsn = rsn;
        }

        public string Type { get; }
        public object Data { get; }
        public uint? Rsn { get; }
    }
}