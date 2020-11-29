using System;

namespace Gang.Demo.Web.Services.Events
{
    public sealed class UserMessageAdded
    {
        public UserMessageAdded(
            string userId,
            string id, string text,
            DateTimeOffset on
            )
        {
            UserId = userId;
            Id = id;
            Text = text;
            On = on;
        }

        public string UserId { get; }
        public string Id { get; }
        public string Text { get; }
        public DateTimeOffset On { get; }
    }
}
