namespace Gang.Web.Services.Commands
{
    public class UpdateUserNameCommand 
    {
        public UpdateUserNameCommand(
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
