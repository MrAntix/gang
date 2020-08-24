namespace Gang.Web.Services.Commands
{
    public class AddMessageCommand : IWebGangCommand
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
