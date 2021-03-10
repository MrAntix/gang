namespace Gang.State.Commands
{
    public sealed class GangNotify
    {
        public GangNotify(
            string id,
            GangNotificationTypes? type = null,
            object data = null
            )
        {
            Id = id;
            Type = type;
            Data = data;
        }

        public string Id { get; }
        public GangNotificationTypes? Type { get; }
        public object Data { get; }
    }
}