using Gang.Web.Services.State;

namespace Gang.Web.Services.Commands
{
    public class UpdateUserCommand: IWebGangUserChange
    {
        public UpdateUserCommand(
            string id,
            string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; }
        public string Name { get; }
    }
}
