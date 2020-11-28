using System;

namespace Gang.Web.Services.State
{
    public sealed class WebGangMessage
    {
        public WebGangMessage(
            string id, string text,
            string userId, DateTimeOffset on)
        {
            Id = id;
            Text = text;
            UserId = userId;
            On = on;
        }

        public string Id { get; }
        public string Text { get; }
        public string UserId { get; }
        public DateTimeOffset On { get; }
    }
}
