namespace Gang.Demo.Web.Server.Commands
{
    public sealed class AddMessage
    {
        public AddMessage(
          string id,
          string text)
        {
            Id = id;
            Text = text;
        }

        public string Id { get; }
        public string Text { get; }
    }
}
