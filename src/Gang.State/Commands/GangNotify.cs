namespace Gang.State.Commands
{
    public sealed class GangNotify
    {
        public GangNotify(
            string id,
            string type, string text
            )
        {
            Id = id;
            Type = type;
            Text = text;
        }

        public string Id { get; }
        public string Type { get; }
        public string Text { get; }
    }
}