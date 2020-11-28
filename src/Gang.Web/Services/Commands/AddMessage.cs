using System;

namespace Gang.Web.Services.Commands
{
    public sealed class AddMessage
    {
        public AddMessage(
          string id,
          string text)
        {
            Id = id;
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public string Id { get; }
        public string Text { get; }
    }
}
