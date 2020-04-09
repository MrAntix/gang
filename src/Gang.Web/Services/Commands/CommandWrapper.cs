using Newtonsoft.Json.Linq;

namespace Gang.Web.Services
{
    public class CommandWrapper
    {
        public CommandWrapper(
            string type,
            JObject command)
        {
            Type = type;
            Command = command;
        }

        public string Type { get; }
        public JObject Command { get; }
    }
}
