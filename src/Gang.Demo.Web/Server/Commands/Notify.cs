namespace Gang.Demo.Web.Server.Commands
{
    public sealed class Notify
    {
        public Notify(
            string type,
            string message = null
            )
        {
            Type = type;
            Message = message;
        }

        public string Type { get; }
        public string Message { get; }
    }
}
