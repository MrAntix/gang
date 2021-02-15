using System;

namespace Gang.Demo.Web.Server.State
{
    public sealed class Message
    {
        public Message(
            string id, string text,
            string byId = null, DateTimeOffset? on = null)
        {
            Id = id;
            Text = text;
            ById = byId;
            On = on ?? DateTimeOffset.Now;
        }

        public string Id { get; }
        public string Text { get; }
        public string ById { get; }
        public DateTimeOffset On { get; }

        public static string EnsureId(string id)
        {
            return string.IsNullOrWhiteSpace(id)
                ? $"{Guid.NewGuid():N}"
                : id;
        }

        public const string ERROR_MESSAGE_TEXT_IS_REQUIRED = "Message text is required";

        public static string TextIsRequred(string text)
        {
            return string.IsNullOrWhiteSpace(text)
                || text == "error"
                ? ERROR_MESSAGE_TEXT_IS_REQUIRED
                : null;
        }
    }
}
