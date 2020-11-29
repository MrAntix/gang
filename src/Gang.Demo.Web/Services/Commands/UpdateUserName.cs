namespace Gang.Demo.Web.Services.Commands
{
    public sealed class UpdateUserName
    {
        public UpdateUserName(
            string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
