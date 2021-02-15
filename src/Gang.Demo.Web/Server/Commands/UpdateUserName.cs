namespace Gang.Demo.Web.Server.Commands
{
    public sealed class UpdateUserName
    {
        public UpdateUserName(
            string name)
        {
            Name = name?.Trim();
        }

        public string Name { get; }
    }
}
