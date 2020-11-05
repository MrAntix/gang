namespace Gang.Web.Services.Commands
{
    public class UpdateUserNameCommand
    {
        public UpdateUserNameCommand(
            string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
