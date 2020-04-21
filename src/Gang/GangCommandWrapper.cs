namespace Gang
{
    public sealed class GangCommandWrapper : IGangCommandWrapper
    {
        public GangCommandWrapper(
            string type,
            object command)
        {
            Type = type;
            Command = command;
        }

        public string Type { get; }
        public object Command { get; }
    }
}