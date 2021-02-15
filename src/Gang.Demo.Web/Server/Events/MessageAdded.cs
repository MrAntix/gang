using System;

namespace Gang.Demo.Web.Server.Events
{
    public sealed class MessageAdded
    {
        public MessageAdded(
            string id, string text,
            string byId,
            DateTimeOffset on
            )
        {
            Id = id;
            Text = text;
            ById = byId;
            On = on;
        }

        public string Id { get; }
        public string Text { get; }
        public string ById { get; }
        public DateTimeOffset On { get; }
    }
}
