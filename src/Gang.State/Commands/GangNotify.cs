namespace Gang.State.Commands
{
    public sealed class GangNotify
    {
        public GangNotify(
            string text = null,
            string id = null,
            GangNotificationTypes? type = null,
            int? timeout = null
            )
        {
            Text = text;
            Id = id;
            Type = type;
            Timeout = timeout;
        }

        public string Text { get; }
        public string Id { get; }
        public GangNotificationTypes? Type { get; }
        public int? Timeout { get; }
    }
}