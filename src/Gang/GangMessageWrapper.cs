namespace Gang
{
    public sealed class GangMessageWrapper
    {
        public GangMessageWrapper(
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