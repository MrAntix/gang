namespace Gang.Web.Services.Commands
{
    public class AddMessageCommand
    {
        public AddMessageCommand(
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
