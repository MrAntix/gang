using System;

namespace Gang.Web.Services.State
{
    public class WebGangMessage
    {
        public WebGangMessage(
            string id,
            DateTimeOffset on,
            string userId,
            string text)
        {
            Id = id;
            On = on;
            UserId = userId;
            Text = text;
        }

        public string Id { get; }
        public DateTimeOffset On { get; }
        public string UserId { get; }
        public string Text { get; }
    }
}
