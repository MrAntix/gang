namespace Gang.Web.Services.Commands
{
    public class UpdateUserName
    {
        public UpdateUserName(
            string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
