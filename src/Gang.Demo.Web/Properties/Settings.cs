namespace Gang.Demo.Web.Properties
{
    public sealed class Settings : IGangSettings
    {
        public AppSettings App { get; set; }
        public SmtpSettings Smtp { get; set; }
        public AuthSettings Auth { get; set; }
        public FileStoreSettings FileStore { get; set; }

        public StateStorageTypes StateStorage { get; set; }

        GangApplication IGangSettings.Application => new(App.Id, App.Name);
    }
}
