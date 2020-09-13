namespace Gang.Web.Services.Commands
{
    public class NotifyCommand
    {
        public const string TYPE_NAME = "notify";

        public NotifyCommand(            
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
