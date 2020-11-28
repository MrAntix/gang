namespace Gang.Web.Services.Commands
{
    public sealed class Notify
    {
        public Notify(
            string type,
            string message
            )
        {
            Type = type;
            Message = message;
        }

        public string Type { get; }
        public string Message { get; }
    }
}
