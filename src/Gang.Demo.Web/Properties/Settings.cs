using Gang.Authentication;
using Gang.Authentication.Crypto;

namespace Gang.Demo.Web.Properties
{
    public sealed class Settings : IGangSettings, IGangAuthenticationSettings
    {
        public AppSettings App { get; set; }
        public SmtpSettings Smtp { get; set; }
        public AuthSettings Auth { get; set; }
        public FileStoreSettings FileStore { get; set; }

        public StateStorageTypes StateStorage { get; set; }

        GangApplication IGangSettings.Application => new(App.Id, App.Name);
        string IGangCryptoSettings.Secret => Auth?.Secret;
        int? IGangAuthenticationSettings.LinkParts => Auth?.LinkParts;
        int? IGangAuthenticationSettings.LinkExpiryMinutes => Auth?.LinkExpiryMinutes;
        int? IGangAuthenticationSettings.SessionExpiryMinutes => Auth.SessionExpiryMinutes;
    }
}
