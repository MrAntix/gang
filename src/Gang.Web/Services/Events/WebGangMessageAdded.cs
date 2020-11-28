using System;

namespace Gang.Web.Services.Events
{
    public sealed class WebGangMessageAdded
    {
        public WebGangMessageAdded(
            string id, string text,
            string userId,
            DateTimeOffset on
            )
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
