namespace Gang.Demo.Web.Server.Commands
{
    public sealed class SetSettings
    {
        public SetSettings(
            bool authEnabled)
        {
            AuthEnabled = authEnabled;
        }

        public bool AuthEnabled { get; }

        public static readonly SetSettings Default = new(true);
    }
}
