namespace Gang.Commands
{
    public sealed class GangCommandWrapper
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